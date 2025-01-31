using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;
using NuGet.Packaging;


namespace MyField.Controllers
{
    public class TransfersController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IMapper _mapper;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public TransfersController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
             IMapper mapper,
             IActivityLogger activityLogger,
             EmailService emailService,
             RoleManager<IdentityRole> roleManager,
             IEncryptionService encryptionService,
             RequestLogService requestLogService)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _roleManager = roleManager;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        [Authorize(Roles = "Sport Administrator, Division Manager")]
        public async Task<IActionResult> TransferPeriod()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var division = await _context.SportMember
                .Where(d => d.Id == user.Id && d.DivisionId == divisionId)
                .Include(d => d.Division)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division?.Division?.DivisionName;

            var transferPeriod = await _context.TransferPeriod
                .Where(t => t.DivisionId == divisionId)
                .Include(t => t.League)
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
                .OrderByDescending(t => t.CreatedDateTime)
                .ToListAsync();

            return View(transferPeriod);
        }


        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> PlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentSeason = await _context.League
                 .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                  .FirstOrDefaultAsync();


            ViewBag.CurrentSeason = currentSeason.LeagueYears;


            return View();
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> PendingTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var pendingTrasfers = await _context.Transfer
                .Where(p => p.Status == TransferStatus.Pending && 
                       p.SellerClub.League.IsCurrent && 
                       p.CustomerClub.League.IsCurrent &&
                       p.League.IsCurrent && 
                       p.DivisionId == divisionId)
                .Include(p => p.SellerClub)
                .Include(p => p.CustomerClub)
                .Include(p => p.Player)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .ToListAsync();

            return PartialView("_PendingPlayerTransfersPartial", pendingTrasfers);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> PaidTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var paidTrasfers = await _context.Transfer
                .Where(p => p.Status == TransferStatus.Completed &&
                       p.SellerClub.League.IsCurrent &&
                       p.CustomerClub.League.IsCurrent &&
                       p.League.IsCurrent &&
                       p.DivisionId == divisionId)
                .Include(p => p.SellerClub)
                .Include(p => p.CustomerClub)
                .Include(p => p.Player)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .ToListAsync();

            return PartialView("_PaidPlayerTransfersPartial", paidTrasfers);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> RejectedTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var rejectedTrasfers = await _context.Transfer
                .Where(p => p.Status == TransferStatus.Rejected &&
                       p.SellerClub.League.IsCurrent &&
                       p.CustomerClub.League.IsCurrent &&
                       p.League.IsCurrent &&
                       p.DivisionId == divisionId)
                .Include(p => p.SellerClub)
                .Include(p => p.CustomerClub)
                .Include(p => p.Player)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .ToListAsync();

            return PartialView("_RejectedPlayerTransfersPartial", rejectedTrasfers);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> AcceptedTransfers()
        {
            var acceptedTrasfers = await _context.Transfer
                .Where(p => p.Status == TransferStatus.Accepted &&
                       p.SellerClub.League.IsCurrent &&
                       p.CustomerClub.League.IsCurrent &&
                       p.League.IsCurrent)
                .Include(p => p.SellerClub)
                .Include(p => p.CustomerClub)
                .Include(p => p.Player)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .ToListAsync();

            return PartialView("_AcceptedPlayerTransfersPartial", acceptedTrasfers);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> FindPlayerTransferMarket()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var division = await _context.Divisions
                .Where(d => d.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division.DivisionName;

            return View();
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyTransferRequestsTabs()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubs = await _context.Club
                                      .FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            ViewBag.ClubName = clubs?.ClubName;

            return View();
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyTransfersTabs()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubs = await _context.Club
                                      .FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            ViewBag.ClubName = clubs?.ClubName;

            return View();
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyPendingPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var pendingTransfers = await _context.Transfer
                .Where(mo => mo.Status == TransferStatus.Pending && 
                mo.SellerClub.ClubId == clubId &&
                 mo.League.IsCurrent)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                 .Include(s => s.CustomerClub)
                .ToListAsync();

            return PartialView("MyPendingTransfers", pendingTransfers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyAcceptedPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var acceptedTransfers = await _context.Transfer
                .Where(mo => mo.Status == TransferStatus.Accepted && 
                mo.SellerClub.ClubId == clubId &&
                mo.SellerClub.League.IsCurrent &&
                mo.CustomerClub.League.IsCurrent &&
                 mo.League.IsCurrent)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                .Include(s => s.CustomerClub)
                .ToListAsync();

            return PartialView("MyAcceptedTransfers", acceptedTransfers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyRejectedPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var rejectedTransfers = await _context.Transfer
                .Where(mo => mo.Status == TransferStatus.Rejected && 
                mo.SellerClub.ClubId == clubId &&
                mo.SellerClub.League.IsCurrent &&
                mo.CustomerClub.League.IsCurrent &&
                 mo.League.IsCurrent)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                .Include(s => s.CustomerClub)
                .ToListAsync();

            return PartialView("MyRejectedTransfers", rejectedTransfers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> PendingPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var pendingTransfers = await _context.Transfer
                .Where(mo => mo.Status == TransferStatus.Pending && 
                mo.CustomerClub.ClubId == clubId &&
                mo.SellerClub.League.IsCurrent &&
                mo.CustomerClub.League.IsCurrent &&
                 mo.League.IsCurrent)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                .ToListAsync();

            return PartialView("PendingTransfers", pendingTransfers);
        }


        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> CancelledPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var pendingTransfers = await _context.Transfer
                .Where(mo => mo.Status == TransferStatus.Cancelled &&
                mo.CustomerClub.ClubId == clubId &&
                mo.SellerClub.League.IsCurrent &&
                mo.CustomerClub.League.IsCurrent &&
                mo.League.IsCurrent)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                .ToListAsync();

            return PartialView("CancelledTransfers", pendingTransfers);
        }


        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> AcceptedPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var acceptedTransfers = await _context.Transfer
                .Where(mo => mo.Status == TransferStatus.Accepted &&
                mo.CustomerClub.ClubId == clubId &&
                mo.SellerClub.League.IsCurrent &&
                mo.CustomerClub.League.IsCurrent &&
                mo.League.IsCurrent)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                .ToListAsync();

            foreach(var acceptedTransfer in acceptedTransfers)
            {
                var playerMarketValue = await _context.Transfer
                    .Where(mo => mo.Status == TransferStatus.Accepted)
                    .Include(s => s.Player)
                    .FirstOrDefaultAsync();

                if(playerMarketValue != null && playerMarketValue.Player.MarketValue != null)
                {
                    ViewBag.PlayerMarketValue = playerMarketValue.Player.MarketValue;
                }

                else
                {
                    ViewBag.PlayerMarketValue = "NA";
                }
            }


            return PartialView("AcceptedTransfers", acceptedTransfers);
        }


        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> RejectedPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var rejectedTransfers = await _context.Transfer
               .Where(mo => mo.Status == TransferStatus.Rejected &&
                mo.CustomerClub.ClubId == clubId &&
                mo.SellerClub.League.IsCurrent &&
                mo.CustomerClub.League.IsCurrent &&
                mo.League.IsCurrent)
               .Include(s => s.Player)
               .Include(s => s.SellerClub)
               .ToListAsync();

            return PartialView("RejectedTransfers", rejectedTransfers);
        }


        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> CompletedPlayerTransfers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var completedTransfers = await _context.Transfer
                     .Where(mo => mo.Status == TransferStatus.Completed &&
                      mo.CustomerClub.ClubId == clubId &&
                      mo.SellerClub.League.IsCurrent &&
                      mo.CustomerClub.League.IsCurrent &&
                      mo.League.IsCurrent)
                     .Include(s => s.Player)
                     .Include(s => s.SellerClub)
                     .ToListAsync();

            return PartialView("CompletedTransfers", completedTransfers);
        }


        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> TransferList()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var division = await _context.SportMember
                .Where(d => d.Id == user.Id && d.DivisionId == divisionId)
                .Include(d => d.Division)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division?.Division?.DivisionName;

            var transferLists = await _context.Transfer
                .Where(t => t.League.IsCurrent && t.DivisionId == divisionId)
                .Include(s => s.SellerClub)
                .Include(s => s.CustomerClub)
                .Include(s => s.CreatedBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.Player)
                .ToListAsync();

            return PartialView(transferLists);
        }


        [Authorize]
        public async Task<IActionResult> TransferMarket()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var division = await _context.SportMember
                .Where(d => d.Id == user.Id && d.DivisionId == divisionId)
                .Include(d => d.Division)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division?.Division?.DivisionName;

            var playerTransferMarket = await _context.PlayerTransferMarket
                .Where(p => !p.IsArchived && p.League.IsCurrent && p.DivisionId == divisionId)
                .Include(s => s.Club)
                .Include(s => s.CreatedBy)
                .Include(s => s.Player)
                .ToListAsync();


            return View(playerTransferMarket);
        }


        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> PlayerTransferMarket()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var division = await _context.SportMember
                .Where(d => d.Id == user.Id && d.DivisionId == divisionId)
                .Include(d => d.Division)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division?.Division?.DivisionName;

            var playerTransferMarket = await _context.PlayerTransferMarket
                .Where(p => !p.IsArchived && p.League.IsCurrent && 
                p.DivisionId == divisionId)
                .Include(s => s.Club)
                .Include(s => s.CreatedBy)
                .Include(s => s.Player)
                .ToListAsync();

            return PartialView("_PlayerTransferMarketPartial", playerTransferMarket);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        public async Task<IActionResult> OpenPlayerTransferPeriod(string seasonCode)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var user = await _userManager.GetUserAsync(User);

                    var divisionId = (user as ClubAdministrator)?.DivisionId ??
                         (user as ClubManager)?.DivisionId ??
                         (user as Player)?.DivisionId ??
                         (user as SportsMember)?.DivisionId ??
                         (user as Officials)?.DivisionId ??
                         (user as DivisionManager)?.DivisionId;


                    var currentSeason = await _context.League
                        .Where(league => league.IsCurrent && league.DivisionId == divisionId)
                        .FirstOrDefaultAsync();

                    var transferPeriod = await _context.TransferPeriod
                        .Where(t => t.IsCurrent && t.DivisionId == divisionId)
                        .FirstOrDefaultAsync();

                    var transferReport = await _context.TransfersReports
                        .Where(t => t.Season.IsCurrent && t.DivisionId == divisionId)
                        .FirstOrDefaultAsync();

                    var currentSeasonCode = currentSeason?.LeagueCode;

                    if (transferPeriod.PeriodOpenCount >= 2)
                    {
                        TempData["Message"] = "Transfer period can only be opened twice per season. Please wait until next season.";
                        return NotFound();
                    }

                    if (currentSeason != null && currentSeasonCode == seasonCode)
                    {
                        var players = await _context.Player
                            .Where(p => p.Club.LeagueId == currentSeason.LeagueId && 
                            p.DivisionId == divisionId)
                            .Include(s => s.Club)
                            .ToListAsync();


                        var userId = user.Id;

                        var transferMarkets = new List<PlayerTransferMarket>();

                        foreach (var player in players)
                        {
                            var transfermarket = new PlayerTransferMarket
                            {
                                PlayerId = player.Id,
                                ClubId = player.ClubId,
                                CreatedById = userId,
                                CreatedDateTime = DateTime.UtcNow,
                                SaleStatus = SaleStatus.Available,
                                LeagueId = currentSeason.LeagueId,
                                DivisionId = divisionId
                            };

                            transferMarkets.Add(transfermarket);
                        }

                        transferPeriod.PeriodOpenCount++;
                        transferPeriod.IsOpened = true;
                        transferPeriod.ModifiedDateTime = DateTime.Now;
                        transferPeriod.ModifiedById = user.Id;

                        await _context.PlayerTransferMarket.AddRangeAsync(transferMarkets);
                        await _context.SaveChangesAsync();

                        int transferMarketCount = await GetTransferMarketCountAsync();
                        transferReport.TransferMarketCount = transferMarketCount;

                        var roleNames = new[] { "Club Administrator", "Club Manager" };
                        var roleUsers = new List<UserBaseModel>();

                        foreach (var roleName in roleNames)
                        {
                            var role = await _roleManager.FindByNameAsync(roleName);
                            if (role != null)
                            {
                                var userIds = await _userManager.GetUsersInRoleAsync(roleName);
                                roleUsers.AddRange(userIds);
                            }
                        }

                        var subject = "Transfer Period Opened";
                        var emailBodyTemplate = $@"
                Dear Club Administrator/Manager,<br/><br/>
                The player transfer period for season <b>{currentSeason.LeagueYears}</b> has been opened.<br/><br/>
                Clubs can now start making player purchases.<br/><br/>
                If you have any questions or need further assistance, please contact us at support@ksfoundation.com.<br/><br/>
                Regards,<br/>
               Diski360 Support Team
                ";

                        foreach (var roleUser in roleUsers.Distinct())
                        {
                            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(roleUser.Email, subject, emailBodyTemplate, "Diski 360"));
                        }

                        TempData["Message"] = $"Transfer period for season {currentSeason.LeagueYears} has been opened successfully and clubs can now start making player purchases";
                        await _activityLogger.Log($"Opened player transfer period for season {currentSeason.LeagueYears}", user.Id);
                        await _requestLogService.LogSuceededRequest("Player transfer period opened successfully.", StatusCodes.Status200OK);

                        return Ok();
                    }
                    else
                    {
                        TempData["Message"] = "Failed to open transfer period. Season code does not match the code of the current season.";
                        await _requestLogService.LogFailedRequest("Failed to open player transfer period due to unmatching season code.", StatusCodes.Status200OK);

                        return Ok();
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return Json(new { success = false, error = "Model state is invalid", errors = errors });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to open transfer period: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }


        public async Task<int> GetTransferMarketCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            return await _context.PlayerTransferMarket
                .Where(p => p.DivisionId == divisionId)
                .CountAsync();
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> ClosePlayerTransferPeriod(string seasonCode)
        {

            try
            {
                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;

                var currentSeason = await _context.League
                    .Where(league => league.IsCurrent && 
                    league.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                var transferPeriod = await _context.TransferPeriod
                    .Where(t => t.IsOpened && 
                    t.DivisionId == divisionId &&
                    t.League.IsCurrent)
                    .FirstOrDefaultAsync();

                if (currentSeason == null)
                {
                    TempData["Message"] = "Failed to close transfer period. No current season found.";
                    return NotFound();
                }

                var currentSeasonCode = currentSeason.LeagueCode;

                if (currentSeasonCode != seasonCode)
                {
                    TempData["Message"] = $"Failed to close transfer period. Season code '{seasonCode}' does not match the code of the current season '{currentSeasonCode}'.";
                    await _requestLogService.LogFailedRequest("Failed to close player transfer period due to unmatching season code.", StatusCodes.Status200OK);

                    return NotFound();
                }

                if(transferPeriod != null)
                {
                    transferPeriod.IsOpened = false;
                    transferPeriod.ModifiedDateTime = DateTime.Now;
                    transferPeriod.ModifiedById = user.Id;
                }

                var invoicesToArchive = await _context.Invoices
                    .Where(it => it.Transfer.DivisionId == divisionId || it.Fine.DivisionId == divisionId)
                    .Include(it => it.Fine)
                    .Include(it => it.CreatedBy)
                    .Include(it => it.Payment)
                    .Include(it => it.Transfer)
                    .ToListAsync();


                foreach (var invoice in invoicesToArchive)
                {
                    var newInvoicesArchives = new Invoices_Archive
                    {
                        CreatedById = invoice.CreatedBy.Id,
                        FineId = invoice.Fine.FineId,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceTimeStamp = invoice.InvoiceTimeStamp,
                        IsEmailed = invoice.IsEmailed,
                        PaymentId = invoice.Payment.PaymentId,
                        TransferId = invoice.Transfer.TransferId,
                        Transfer = invoice.Transfer
                    };

                    _context.Invoices_Archives.Add(newInvoicesArchives);
                    await _context.SaveChangesAsync();
                }


                var transferRecordsToArchive = await _context.Transfer
                   .Where(rta => rta.DivisionId == divisionId && rta.LeagueId == currentSeason.LeagueId)
                   .Include(rta => rta.Player)
                   .Include(rta => rta.PlayerTransferMarket)
                    .Include(rta => rta.League)
                    .Include(rta => rta.SellerClub)
                    .Include(rta => rta.CustomerClub)
                     .Include(rta => rta.CreatedBy)
                     .Include(rta => rta.Division)
                     .Include(rta => rta.Approved_Declined_By)
                     .Include(rta => rta.ModifiedBy)
                   .ToListAsync();

                foreach (var y in transferRecordsToArchive)
                {
                    var newTransferArchives = new Transfer_Archive
                    {
                        CreatedById = y.CreatedBy.Id,
                        CreatedDateTime = y.CreatedDateTime,
                        PlayerId = y.Player.Id,
                        LeagueId = y.LeagueId,
                        PlayerTransferMarketId = y.PlayerTransferMarket.PlayerTransferMarketId,
                        DivisionId = y.Division.DivisionId,
                        Approved_Declined_ById = y.Approved_Declined_By.Id,
                        CustomerClubId = y.CustomerClubId,
                        paymentTransferStatus = y.paymentTransferStatus,
                        ModifiedById = y.ModifiedBy.Id,
                        ModifiedDateTime = y.ModifiedDateTime,
                        SellerClubId = y.SellerClub.ClubId,
                        TransferId = y.TransferId,
                        Approved_Declined_By = y.Approved_Declined_By,
                        CreatedBy = y.CreatedBy,
                        CustomerClub = y.CustomerClub,
                        Division = y.Division,
                        League = y.League,
                        ModifiedBy = y.ModifiedBy,
                        Player = y.Player,
                        PlayerTransferMarket = y.PlayerTransferMarket,
                        SellerClub = y.SellerClub,
                        Status = y.Status
                    };
                    _context.Transfer_Archives.Add(newTransferArchives);
                    await _context.SaveChangesAsync();
                }


                var recordsToArchive = await _context.PlayerTransferMarket
                    .Where(rta => rta.DivisionId == divisionId && rta.LeagueId == currentSeason.LeagueId)
                    .Include(rta => rta.Player)
                    .Include(rta => rta.Club)
                     .Include(rta => rta.League)
                      .Include(rta => rta.CreatedBy)
                    .ToListAsync();

                foreach (var y in recordsToArchive)
                {
                    var transferMarketArchives = new PlayerTransferMarket_Archive
                    {
                        CreatedById = y.CreatedBy.Id,
                        ClubId = y.Club.ClubId,
                        CreatedDateTime = y.CreatedDateTime,
                        PlayerId = y.Player.Id,
                        LeagueId = y.League.LeagueId,
                        PlayerTransferMarketId = y.PlayerTransferMarketId,
                        SellingPrice = y.SellingPrice,
                        SaleStatus = y.SaleStatus,
                        IsArchived = true,
                        DivisionId = y.DivisionId,
                        CreatedBy = y.CreatedBy,
                        Club = y.Club,
                        Division = y.Division,
                        League = y.League,
                        Player = y.Player,
                        Transfers = y.Transfers
                    };
                    _context.PlayerTransferMarket_Archives.Add(transferMarketArchives);
                    await _context.SaveChangesAsync();
                }

                _context.Invoices.RemoveRange(invoicesToArchive);
                _context.Transfer.RemoveRange(transferRecordsToArchive);
                _context.PlayerTransferMarket.RemoveRange(recordsToArchive);
                await _context.SaveChangesAsync();
                await _activityLogger.Log($"Closed player transfer period for season {currentSeason.LeagueYears}", user.Id);
                TempData["Message"] = $"Transfer period for season {currentSeason.LeagueYears} has been ended and no player purchases are allowed.";
                await _requestLogService.LogSuceededRequest("Player transfer period closed successfully.", StatusCodes.Status200OK);

                return Ok();
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }



        public async Task<IActionResult> ValidateSeasonCode(string seasonCode)
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentSeason = await _context.League
                .FirstOrDefaultAsync(league => league.IsCurrent && league.DivisionId == divisionId);

            var isValid = currentSeason != null && currentSeason.LeagueCode == seasonCode;

            return Json(new { isValid = isValid });
        }


        [Authorize(Roles = ("Club Administrator"))]
        [HttpGet]
        public async Task<IActionResult> InitiatePlayerTransfer(string playerId, string marketId, string clubId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;


                var decryptedPlayerId = _encryptionService.Decrypt(playerId);
                var decryptedClubId = _encryptionService.DecryptToInt(clubId);
                var decryptedMarketId = _encryptionService.DecryptToInt(marketId);


                var player = await _context.Player
                    .Where(mo => mo.Id == decryptedPlayerId && 
                    mo.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                var playerClub = await _context.Club
                    .Where(mo => mo.ClubId == decryptedClubId && 
                    mo.DivisionId == divisionId)
                    .FirstOrDefaultAsync();


                if (player == null)
                {
                    TempData["Errors"] = new List<string> { "Player not found." };
                    return RedirectToAction("ErrorPage", "Home", new { errorMessage = "Player not found." + $"PlayerId  = {playerId}" });
                }

                var transferMarket = await _context.PlayerTransferMarket
                    .Where(mo => mo.PlayerTransferMarketId == decryptedMarketId &&
                    mo.League.IsCurrent &&
                    mo.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (transferMarket == null)
                {
                    TempData["Errors"] = new List<string> { "Transfer market not found." };
                    return RedirectToAction("ErrorPage", "Home", new { errorMessage = "Transfer market not found." });
                }

                if (playerClub == null)
                {
                    TempData["Errors"] = new List<string> { "Player club not found." };
                    return RedirectToAction("ErrorPage", "Home", new { errorMessage = "Player club not found." });
                }

                var viewModel = new InitiatePlayerTransferViewModel
                {
                    LeagueId = transferMarket.LeagueId,
                    MarketId = decryptedMarketId,
                    PlayerId = decryptedPlayerId,
                    SellerClubId = decryptedClubId,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    Position = player.Position,
                    ProfilePicture = player.ProfilePicture,
                    JerseyNumber = player.JerseyNumber,
                    DateOfBirth = player.DateOfBirth,
                    MarketValue = player.MarketValue,
                    ClubName = playerClub.ClubName,
                    ClubBadge = playerClub.ClubBadge,
                };

                ViewBag.Positions = Enum.GetValues(typeof(Position))
                    .Cast<Position>()
                    .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });
                return View(viewModel);
            }
            catch (Exception ex)
            {
                string errorMessage = "An error occurred while processing the request: " + ex.Message;
                return RedirectToAction("ErrorPage", "Home", new { errorMessage = errorMessage });
            }
        }


        [Authorize(Roles = "Club Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitiatePlayerTransfer(InitiatePlayerTransferViewModel viewModel)
        {
            try
            {
                var loggedInUser = await _userManager.GetUserAsync(User);

                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                                 (user as ClubManager)?.DivisionId ??
                                 (user as Player)?.DivisionId ??
                                 (user as SportsMember)?.DivisionId ??
                                 (user as Officials)?.DivisionId ??
                                 (user as DivisionManager)?.DivisionId;

                if (loggedInUser == null || !(loggedInUser is ClubAdministrator clubAdministrator))
                {
                    TempData["Errors"] = new List<string> { "User is not authorized" };
                    return View(viewModel);
                }

                var transferMarket = await _context.PlayerTransferMarket
                    .Where(mo => mo.PlayerTransferMarketId == viewModel.MarketId &&
                                 mo.DivisionId == divisionId)
                    .Include(s => s.Player)
                    .Include(s => s.Club)
                    .FirstOrDefaultAsync();

                if (transferMarket == null)
                {
                    TempData["Errors"] = new List<string> { "Transfer market not found" };
                    return RedirectToAction("ErrorPage", "Home", new { errorMessage = "Transfer market not found." });
                }

                if (transferMarket.Player == null)
                {
                    TempData["Errors"] = new List<string> { "Player not found in the transfer market" };
                    return RedirectToAction("ErrorPage", "Home", new { errorMessage = "Player not found in the transfer market." });
                }

                var existingPlayerTransferForClub = await _context.Transfer
                    .Where(e => e.PlayerTransferMarketId == transferMarket.PlayerTransferMarketId &&
                                e.Player.Id == transferMarket.Player.Id)
                    .Include(e => e.Player)
                    .Include(e => e.CustomerClub)
                    .Include(e => e.League)
                    .FirstOrDefaultAsync();

                if (existingPlayerTransferForClub != null &&
                    existingPlayerTransferForClub.CustomerClub.ClubId == clubAdministrator.ClubId &&
                    existingPlayerTransferForClub.Player.Id == transferMarket.Player.Id &&
                    existingPlayerTransferForClub.League.IsCurrent)
                {
                    TempData["Message"] = $"You have already initiated transfer communications for {existingPlayerTransferForClub.Player.FirstName} {existingPlayerTransferForClub.Player.LastName}. Please try signing other players.";
                    return RedirectToAction(nameof(TransferMarket));
                }

                if (transferMarket.Player.ClubId == clubAdministrator.ClubId)
                {
                    TempData["Message"] = "You can't initiate transfer communication for your own player! Please select other clubs' players.";
                    return RedirectToAction(nameof(TransferMarket));
                }

                var transfer = await _context.Transfer
                    .Where(t => t.PlayerId == viewModel.PlayerId && t.League.IsCurrent &&
                               t.DivisionId == divisionId)
                    .Include(t => t.Player)
                    .Include(t => t.CustomerClub)
                    .FirstOrDefaultAsync();

                if (transfer != null && transfer.Status == TransferStatus.Accepted)
                {
                    TempData["Message"] = $"You cannot initiate transfer communication for this player since it has been already accepted for another buyer club.";
                    return RedirectToAction(nameof(TransferMarket));
                }

                if (transferMarket.SaleStatus == SaleStatus.Unavailable)
                {
                    TempData["Message"] = "This player has signed a new contract with another club. Please try signing available players.";
                    return RedirectToAction(nameof(TransferMarket));
                }

                var newPlayerTransfer = new Transfer
                {
                    LeagueId = viewModel.LeagueId,
                    PlayerTransferMarketId = transferMarket.PlayerTransferMarketId,
                    PlayerId = viewModel.PlayerId,
                    CustomerClubId = clubAdministrator.ClubId,
                    SellerClubId = viewModel.SellerClubId,
                    CreatedDateTime = DateTime.Now,
                    CreatedById = loggedInUser.Id,
                    ModifiedById = loggedInUser.Id,
                    ModifiedDateTime = DateTime.Now,
                    Approved_Declined_ById = loggedInUser.Id,
                    Status = TransferStatus.Pending,
                    DivisionId = divisionId
                };

                _context.Transfer.Add(newPlayerTransfer);
                await _context.SaveChangesAsync();

                var sellerClubTransferReport = await _context.ClubTransferReports
                    .Where(c => c.ClubId == viewModel.SellerClubId && c.League.IsCurrent &&
                               c.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                var buyerClubTransferReport = await _context.ClubTransferReports
                    .Where(c => c.ClubId == clubAdministrator.ClubId && c.League.IsCurrent &&
                               c.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (sellerClubTransferReport != null)
                {
                    sellerClubTransferReport.IncomingTransfersCount++;
                    _context.Update(buyerClubTransferReport);
                }

                if (buyerClubTransferReport != null)
                {
                    buyerClubTransferReport.OutgoingTransfersCount++;
                    _context.Update(sellerClubTransferReport);
                }

                await _context.SaveChangesAsync();

                var sellerClubAdmin = await _context.ClubAdministrator
                    .Where(u => u.ClubId == viewModel.SellerClubId && u.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                var sellerClub = await _context.Club
                    .Where(c => c.ClubId == viewModel.SellerClubId && c.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (sellerClubAdmin != null)
                {
                    var subject = "Player Transfer Request Notification";
                    var emailBodyTemplate = $@"
            Dear Club Administrator,<br/><br/>
            A new transfer request has been initiated for the player <b>{viewModel.FirstName} {viewModel.LastName}</b>.<br/><br/>
            <b>Transfer Details:</b><br/>
            - Market Value: {viewModel.MarketValue:C}<br/>
            - Position: {viewModel.Position}<br/>
            - Club Name: {viewModel.ClubName}<br/><br/>
            Please review the transfer request in your dashboard.<br/><br/>
            If you have any questions or need further assistance, please contact us at support@diski360.com.<br/><br/>
            Regards,<br/>
            Diski360 Management
            ";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(sellerClubAdmin.Email, subject, emailBodyTemplate, "Diski 360"));
                }

                if (sellerClub != null && !string.IsNullOrEmpty(sellerClub.Email))
                {
                    var clubEmailSubject = "New Player Transfer Request";
                    var clubEmailBodyTemplate = $@"
            Dear {sellerClub.ClubName} Management,<br/><br/>
            A new transfer request has been initiated for the player <b>{viewModel.FirstName} {viewModel.LastName}</b>.<br/><br/>
            <b>Transfer Details:</b><br/>
            - Market Value: {viewModel.MarketValue:C}<br/>
            - Position: {viewModel.Position}<br/>
            - Club Name: {viewModel.ClubName}<br/><br/>
            Please review the transfer request in your dashboard.<br/><br/>
            If you have any questions or need further assistance, please contact us at support@diski360.com.<br/><br/>
            Regards,<br/>
            Diski360 Management
            ";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(sellerClub.Email, clubEmailSubject, clubEmailBodyTemplate, "Diski 360"));
                }

                TempData["Message"] = $"You have successfully initiated communication with {viewModel.ClubName} for {viewModel.FirstName} {viewModel.LastName}'s transfer.";

                await _requestLogService.LogSuceededRequest("Player transfer initiated successfully.", StatusCodes.Status200OK);

                return RedirectToAction(nameof(TransferMarket));
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to process player transfer: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }




        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> AcceptPlayerTransfer(int transferId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);


            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var transfer = await _context.Transfer
                .Where(t => t.League.IsCurrent && t.DivisionId == divisionId)
                .Include(t => t.Player)
                .Include(t => t.CustomerClub)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                return NotFound();
            }

            if (loggedInUser != null)
            {
                transfer.Status = TransferStatus.Accepted;
                transfer.paymentTransferStatus = PaymentTransferStatus.Pending_Payment;
                transfer.Approved_Declined_ById = loggedInUser.Id;
                transfer.ModifiedDateTime = DateTime.Now;
            }

            _context.Update(transfer);
            await _context.SaveChangesAsync();

            var customerClubAdmin = await _context.ClubAdministrator
                .Where(ca => ca.ClubId == transfer.CustomerClubId && 
                ca.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            var customerClub = await _context.Club
                .Where(c => c.ClubId == transfer.CustomerClubId && 
                c.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            var subject = "Player Transfer Accepted Notification";
            var emailBodyTemplate = $@"
        Dear {transfer.CustomerClub?.ClubName} Management,<br/><br/>
        The transfer request for player <b>{transfer.Player?.FirstName} {transfer.Player?.LastName}</b> has been accepted.<br/><br/>
        <b>Transfer Details:</b><br/>
        - Player: {transfer.Player?.FirstName} {transfer.Player?.LastName}<br/>
        - Customer Club: {transfer.CustomerClub?.ClubName}<br/><br/>
        Please proceed with the next steps as outlined in your dashboard.<br/><br/>
        If you have any questions or need further assistance, please contact us at support@diski360.com.<br/><br/>
        Regards,<br/>
       Diski360 Management
    ";

            if (customerClubAdmin != null && !string.IsNullOrEmpty(customerClubAdmin.Email))
            {
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(customerClubAdmin.Email, subject, emailBodyTemplate, "Diski 360"));
            }

            if (customerClub != null && !string.IsNullOrEmpty(customerClub.Email))
            {
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(customerClub.Email, subject, emailBodyTemplate, "Diski 360"));
            }

            string message = $"You have successfully accepted player transfer for ";

            if (User.IsInRole("Personnel Administrator"))
            {
                if (transfer.Player != null && transfer.CustomerClub != null)
                {
                    message += $"{transfer.Player.FirstName} {transfer.Player.LastName} with {transfer.CustomerClub.ClubName}!";
                }
                else
                {
                    message += "transfer!";
                }

                TempData["Message"] = message;
                await _requestLogService.LogSuceededRequest("Player transfer accepted successfully.", StatusCodes.Status200OK);

                return RedirectToAction(nameof(PlayerTransfers));
            }
            else if (User.IsInRole("Club Administrator"))
            {
                if (transfer.Player != null && transfer.CustomerClub != null)
                {
                    message += $"{transfer.Player.FirstName} {transfer.Player.LastName} with {transfer.CustomerClub.ClubName}!";
                }
                else
                {
                    message += "transfer!";
                }

                TempData["Message"] = message;
                await _requestLogService.LogSuceededRequest("Player transfer accepted successfully.", StatusCodes.Status200OK);

                return RedirectToAction(nameof(MyTransferRequestsTabs));
            }

            return View();
        }


        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> RejectPlayerTransfer(int transferId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var transfer = await _context.Transfer
                .Where(t => t.League.IsCurrent && 
                t.DivisionId == divisionId)
                .Include(t => t.Player)
                .Include(t => t.CustomerClub)
                .Include(t => t.SellerClub)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            var sellerClubTransferReport = await _context.ClubTransferReports
                .Where(c => c.ClubId == transfer.SellerClub.ClubId &&
                c.League.IsCurrent && c.DivisionId == divisionId)
                .Include(c => c.Club)
                .FirstOrDefaultAsync();

            var buyerClubTransferReport = await _context.ClubTransferReports
                .Where(c => c.ClubId == transfer.CustomerClub.ClubId &&
                c.League.IsCurrent && c.DivisionId == divisionId)
                .Include(c => c.Club)
                .FirstOrDefaultAsync();

            if (transfer == null)
            {
                return NotFound();
            }

            if (loggedInUser != null)
            {
                transfer.Approved_Declined_ById = loggedInUser.Id;
                transfer.ModifiedDateTime = DateTime.Now;
                transfer.Status = TransferStatus.Rejected;
            }

            buyerClubTransferReport.RejectedOutgoingTransfersCount++;
            sellerClubTransferReport.RejectedIncomingTransfersCount++;

            _context.Update(transfer);
            await _context.SaveChangesAsync();

            var customerClubAdmin = await _context.ClubAdministrator
                .Where(ca => ca.ClubId == transfer.CustomerClub.ClubId && ca.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            var sellerClubAdmin = await _context.ClubAdministrator
                .Where(ca => ca.ClubId == transfer.SellerClub.ClubId && ca.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            var subject = "Player Transfer Rejected Notification";
            var emailBodyTemplate = $@"
        Dear {transfer.CustomerClub?.ClubName} Management,<br/><br/>
        The transfer request for player <b>{transfer.Player?.FirstName} {transfer.Player?.LastName}</b> has been rejected.<br/><br/>
        <b>Transfer Details:</b><br/>
        - Player: {transfer.Player?.FirstName} {transfer.Player?.LastName}<br/>
        - Seller Club: {transfer.SellerClub?.ClubName}<br/>
        - Customer Club: {transfer.CustomerClub?.ClubName}<br/><br/>
        Please review the transfer details and contact us if you have any questions.<br/><br/>
        If you need further assistance, please contact us at support@diski360.com.<br/><br/>
        Regards,<br/>
       Diski360 Management
    ";

            if (customerClubAdmin != null && !string.IsNullOrEmpty(customerClubAdmin.Email))
            {
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(customerClubAdmin.Email, subject, emailBodyTemplate, "Diski 360"));
            }

            if (sellerClubAdmin != null && !string.IsNullOrEmpty(sellerClubAdmin.Email))
            {
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(sellerClubAdmin.Email, subject, emailBodyTemplate, "Diski 360"));
            }

            string message = $"You have rejected player transfer communication for ";

            if (transfer.Player != null && transfer.CustomerClub != null)
            {
                message += $"{transfer.Player.FirstName} {transfer.Player.LastName} with {transfer.CustomerClub.ClubName}!";
            }
            else
            {
                message += "transfer!";
            }

            TempData["Message"] = message;
            await _requestLogService.LogSuceededRequest("Player transfer rejected successfully.", StatusCodes.Status200OK);

            return RedirectToAction(nameof(MyTransferRequestsTabs));
        }

        [Authorize(Roles = ("Club Administrator"))]
        [HttpPost]
        public async Task<IActionResult> CancelPlayerTransfer(int transferId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            var transfer = await _context.Transfer
                .Include(t => t.Player)
                .Include(t => t.SellerClub)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);

            if (transfer == null)
            {
                return NotFound();
            }

            if(loggedInUser != null)
            {

            transfer.Approved_Declined_ById = loggedInUser.Id;
            transfer.ModifiedDateTime = DateTime.Now;   
            transfer.Status = TransferStatus.Cancelled;

            }

            _context.Update(transfer);
            await _context.SaveChangesAsync();


            string message = $"You have cancelled player transfer communication for ";
            if (transfer.Player != null && transfer.SellerClub != null)
            {
                message += $"{transfer.Player.FirstName} {transfer.Player.LastName} with {transfer.SellerClub.ClubName}!";
            }
            else
            {
                message += "transfer!";
            }

            TempData["Message"] = message;
            await _requestLogService.LogSuceededRequest("Player transfer cancelled successfully.", StatusCodes.Status200OK);

            return RedirectToAction(nameof(MyTransfersTabs));
        }

        public async Task<IActionResult> PaymentPlayerTransfer()
        {
            return View();
        }
    }
}
