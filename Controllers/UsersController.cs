using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;
using MyField.ViewModels;
using MyField.Services;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using MyField.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CodeFixes;

namespace MyField.Controllers
{
    public class UsersController : Controller
    {
        public readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public UsersController(Ksans_SportsDbContext context,
           FileUploadService fileUploadService,
           UserManager<UserBaseModel> userManager,
           IActivityLogger activityLogger,
           EmailService emailService,
           IEncryptionService encryptionService,
           RequestLogService requestLogService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }




        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> NetUsers(int page = 1, int pageSize = 50)
        {
            var totalUsers = await _context.UserBaseModel.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            var netUsers = await _context.UserBaseModel
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new NetUsersViewModel
            {
                Users = netUsers,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return View(viewModel);
        }




        [Authorize]
        public async Task<IActionResult> AllDivisionManagers(string divisionId)
        {

            ViewBag.DivisionId = divisionId;

            return PartialView("DivisionManagersPartial");
        }

        [Authorize]
        public async Task<IActionResult> AllPlayers(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var players = await _context.Player
                .Where(p => !p.IsDeleted && p.DivisionId == decryptedDivisionId)
                .Include(p => p.Club)
                .ToListAsync();

            var division = await _context.Divisions
               .Where(d => d.DivisionId == decryptedDivisionId)
               .FirstOrDefaultAsync();

            ViewBag.DivisionName = division.DivisionName;

            return View(players);
        }

        [Authorize]
        public async Task<IActionResult> Players(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var players = await _context.Player
                .Where(p => !p.IsDeleted && p.DivisionId == decryptedDivisionId)
                .Include(p => p.Club)
                .ToListAsync();

            ViewBag.DivisionId = decryptedDivisionId;

            return PartialView("_PlayersPartial", players);
        }


        [Authorize]
        public async Task<IActionResult> AllDivisionPlayers(string divisionId)
        {
            ViewBag.DivisionId = divisionId;

            return PartialView("DivisionPlayersPartial");
        }


        [Authorize]
        public async Task<IActionResult> Managers(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var managers = await _context.Club
                .Where(p => p.ClubManager != null && !p.ClubManager.IsDeleted && !p.ClubManager.IsContractEnded && p.DivisionId == decryptedDivisionId)
                .Include(p => p.ClubManager)
                .ToListAsync();

            return PartialView("_ManagersPartial", managers);
        }



        [Authorize]
        [HttpGet]

        public async Task<IActionResult> PasswordAndSecurity()
        {
            var user = await _userManager.GetUserAsync(User);

            var userRole = await _context.UserRoles
                 .Where(ur => ur.UserId == user.Id)
                 .Join(_context.Roles,
                 ur => ur.RoleId,
                 r => r.Id,
                 (ur, r) => r.Name)
                 .FirstOrDefaultAsync();

            var viewModel = new PrivacyAndSecurityViewModel
            {
                UserRole = userRole,
                UserId = user.Id,
                ProfilePicture = user.ProfilePicture,
                FullNames = $"{user.FirstName} {user.LastName}",
            };

            return View(viewModel);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserProfile(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var encryptionService = HttpContext.RequestServices.GetRequiredService<IEncryptionService>();

            string decryptedUserId;
            try
            {
                decryptedUserId = encryptionService.Decrypt(userId);
            }
            catch
            {
                return BadRequest("Invalid user ID.");
            }

            var userProfile = await _context.UserBaseModel
                .Where(u => u.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            if (userProfile == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                UserId = decryptedUserId,
                Names = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = userProfile.Email,
                Phone = userProfile.PhoneNumber,
                ProfilePicture = userProfile.ProfilePicture,
                DateOfBirth = userProfile.DateOfBirth
            };

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == decryptedUserId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            ViewBag.UserRole = userRole;

            var divisionManager = await _context.DivisionManagers   
                .Where(dm => dm.Id == decryptedUserId)
                .Include(dm => dm.Division)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = divisionManager?.Division?.DivisionName;
           
            return View(viewModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PlayerProfile(string userId)
        {
            var decryptedPlayerId = _encryptionService.Decrypt(userId);

            var user = await _userManager.GetUserAsync(User);

            var userSubscription = await _context.Subscriptions
                .Where(us => us.UserId == user.Id)
                .FirstOrDefaultAsync();

            ViewBag.UserSubscription = userSubscription.SubscriptionPlan;

            if (decryptedPlayerId == null)
            {
                return NotFound();
            }

            var userProfile = await _context.Player
                .Where(u => u.Id == decryptedPlayerId)
                .Include(u => u.Club)
                .FirstOrDefaultAsync();


            var playerStats = await _context.PlayerPerformanceReports
                .Where(ps => ps.PlayerId == decryptedPlayerId && ps.League.IsCurrent)
                .Include(ps => ps.Player)
                .FirstOrDefaultAsync();

            var playerProfileViewModel = new PlayerProfileViewModel
            {
                UserId = decryptedPlayerId,
                Names = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = userProfile.Email,
                Phone = userProfile.PhoneNumber,
                ProfilePicture = userProfile.ProfilePicture,
                JerseyNumber = userProfile.JerseyNumber,
                Position = userProfile.Position,
                DateOfBirth = userProfile.DateOfBirth,
                
            };


            var playerStatsViewModel = new PlayerStatsViewModel
            {
                Appearances = playerStats.AppearancesCount,
                Goals = playerStats.GoalsScoredCount,
                Assists = playerStats.AssistsCount,
                YellowCards = playerStats.YellowCardCount,
                RedCards = playerStats.RedCardCount,
                OwnGoals = playerStats.OwnGoalsScoredCount
            };


            var combinedPlayerViewModel = new CombinedPlayerViewModel
            {
                PlayerStatsViewModel = playerStatsViewModel,
                PlayerProfileViewModel = playerProfileViewModel
            };

            var userRole = await _context.UserRoles
                 .Where(ur => ur.UserId == decryptedPlayerId)
                 .Join(_context.Roles,
                 ur => ur.RoleId,
                 r => r.Id,
                 (ur, r) => r.Name)
                 .FirstOrDefaultAsync();

            var club = userProfile.Club;

            ViewBag.Club = club.ClubName;

            ViewBag.UserRole = userRole;

            return View(combinedPlayerViewModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserBaseModel
                .Where(u => u.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            var viewModel = new ProfileViewModel
            {
                UserId = decryptedUserId,
                Names = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = userProfile.Email,
                Phone = userProfile.PhoneNumber,
                ProfilePicture = userProfile.ProfilePicture,
                DateOfBirth = userProfile.DateOfBirth
            };

            var userRole = await _context.UserRoles
                 .Where(ur => ur.UserId == decryptedUserId)
                 .Join(_context.Roles,
                 ur => ur.RoleId,
                 r => r.Id,
                 (ur, r) => r.Name)
                 .FirstOrDefaultAsync();

            if (userRole == "Club Administrator")
            {
                var clubAdministrator = _context.ClubAdministrator
                .Include(c => c.Club)
                .Where(c => c.Id == decryptedUserId)
                .FirstOrDefault();
                ViewBag.Club = clubAdministrator?.Club?.ClubName;
            }
            else if (userRole == "Club Manager")
            {
                var clubManager = _context.ClubManager
               .Include(p => p.Club)
               .Where(c => c.Id == decryptedUserId)
               .FirstOrDefault();
                ViewBag.Club = clubManager?.Club?.ClubName;
            }

            ViewBag.UserRole = userRole;

            return View(viewModel);
        }

        [Authorize(Roles = ("Fans Administrator"))]
        public async Task<IActionResult> DivisionFans()
        {

            var userIdsWithRoles = await _context.UserRoles
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            var usersWithNoRoles = await _context.UserBaseModel
                .Where(u => !userIdsWithRoles.Contains(u.Id))
                .ToListAsync();

            return View(usersWithNoRoles);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> SportAdministrators()
        {

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.SportMember
                    .Where(lu => lu.Id == user.Id)
                    .Include(lu => lu.Division)
                    .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;


            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Sport Administrator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var sportAdministrators = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false && u.DivisionId == divisionId)
                .ToListAsync();

            return View(sportAdministrators);
        }

        [Authorize(Roles = ("System Administrator"))]
        public async Task<IActionResult> SportManagers()
        {

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Division Manager");

            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var sportManagers = await _context.DivisionManagers
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false)
                .Include(u => u.Division)
                .ToListAsync();

            return View(sportManagers);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> SportCoordinators()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.SportMember
                  .Where(lu => lu.Id == user.Id)
                  .Include(lu => lu.Division)
                  .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Sport Coordinator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var sportCoordinators = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                 u.IsDeleted == false &&
                 u.DivisionId == divisionId)
                .ToListAsync();

            return View(sportCoordinators);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> Officials()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.SportMember
                 .Where(lu => lu.Id == user.Id)
                  .Include(lu => lu.Division)
                  .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Official");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var officials = await _context.Officials
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .ToListAsync();

            return View(officials);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> ClubAdministrators()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.SportMember
                 .Where(lu => lu.Id == user.Id)
                 .Include(lu => lu.Division)
                 .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Club administrator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var clubAdministrators = await _context.ClubAdministrator
                .Where(u => userIds.Contains(u.Id) &&
                 u.IsDeleted == false &&
                 u.DivisionId == divisionId)
                .Include(u => u.Club)
                .ToListAsync();

            return View(clubAdministrators);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> ClubManagers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var loggedInUser = await _context.SportMember
                 .Where(lu => lu.Id == user.Id)
                  .Include(lu => lu.Division)
                  .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Club manager");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var clubManagers = await _context.ClubManager
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .Include(u => u.Club)
                .ToListAsync();

            return View(clubManagers);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> DivisionPlayers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.SportMember
              .Where(lu => lu.Id == user.Id)
               .Include(lu => lu.Division)
               .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Player");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var divisionPlayers = await _context.Player
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .Include(u => u.Club)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();


            ViewBag.Clubs = new SelectList(await _context.Club.Where(mo => mo.League.IsCurrent && mo.DivisionId == divisionId).ToListAsync(), "ClubId", "ClubName");


            return View(divisionPlayers);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> NewsAdministrators()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var loggedInUser = await _context.SportMember
                .Where(lu => lu.Id == user.Id)
                 .Include(lu => lu.Division)
                 .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "News administrator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var newsAdministrators = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .ToListAsync();

            return View(newsAdministrators);
        }

        [Authorize(Roles = "Division Manager")]
        public async Task<IActionResult> PersonnelAdministrators()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId ??
                             (user as SportsMember)?.DivisionId ??
                             (user as Officials)?.DivisionId??
                             (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.DivisionManagers
                    .Where(lu => lu.Id == user.Id)
                    .Include(lu => lu.Division)
                    .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Personnel Administrator");
            var userIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

            var personnelAdministrators = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                            u.IsDeleted == false &&
                            u.DivisionId == divisionId)
                .ToListAsync();

            return View(personnelAdministrators);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> SystemAdministrators()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var loggedInUser = await _context.SportMember
                 .Where(lu => lu.Id == user.Id)
                  .Include(lu => lu.Division)
                  .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "System Administrator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var newsUpdaters = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .ToListAsync();

            return View(newsUpdaters);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> NewsUpdaters()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var loggedInUser = await _context.SportMember
                 .Where(lu => lu.Id == user.Id)
                  .Include(lu => lu.Division)
                  .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "News updator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var newsUpdaters = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .ToListAsync();

            return View(newsUpdaters);
        }

        [Authorize(Roles = ("System Administrator"))]
        public async Task<IActionResult> FansAdministrators()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Fans administrator");
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToListAsync();

            var fansAdministrators = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                u.IsDeleted == false &&
                u.DivisionId == divisionId)
                .ToListAsync();

            return View(fansAdministrators);
        }

        [Authorize(Roles = ("System Administrator"))]
        public async Task<IActionResult> SystemUsers()
        {
            var systemUsers = await _context.Users
                .ToListAsync();

            return View(systemUsers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyClubPlayers()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubPlayers = await _context.Player
                .Where(p => p.ClubId == clubAdministrator.ClubId &&
                 p.IsDeleted == false)
                .Include(s => s.Club)
                .ToListAsync();

            ViewBag.ClubName = clubAdministrator?.Club?.ClubName;

            return View(clubPlayers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyClubManagers()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubManagers = await _context.ClubManager
                .Where(p => p.ClubId == clubAdministrator.ClubId &&
                p.IsDeleted == false)
                .Include(s => s.Club)
                .ToListAsync();

            ViewBag.ClubName = clubAdministrator?.Club?.ClubName;

            return PartialView(clubManagers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> ClubPlayers()
        {
            var clubPlayers = await _context.Player
                .Where(s => s.IsDeleted == false)
                .Include(s => s.Club)
                .ToListAsync();

            return View(clubPlayers);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyClubAdministrators()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubAdministrators = await _context.ClubAdministrator
                .Where(p => p.ClubId == clubAdministrator.ClubId &&
                p.IsDeleted == false)
                .Include(s => s.Club)
                .ToListAsync();

            ViewBag.ClubName = clubAdministrator?.Club?.ClubName;

            return View(clubAdministrators);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UpdateProfile(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (string.IsNullOrEmpty(decryptedUserId))
            {
                return NotFound();
            }

            if (decryptedUserId == null || _context.UserBaseModel == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserBaseModel.FindAsync(decryptedUserId);

            if (userProfile == null)
            {
                return NotFound();
            }

            var profileViewModel = new UpdateProfileViewModel
            {
                Id = decryptedUserId,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                PhoneNumber = userProfile.PhoneNumber,
                ProfilePicture = userProfile.ProfilePicture,
                Email = userProfile.Email,
            };

            return View(profileViewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel viewModel, IFormFile ProfilePictureFile)
        {
            var logMessages = new List<string>();

            if(viewModel.Id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);


            var userProfile = await _context.UserBaseModel.FindAsync(viewModel.Id);

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == viewModel.Id)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            if (ValidateProfileUpdatedProperties(viewModel))
            {
                try
                {
                    if (userProfile == null)
                    {
                        logMessages.Add($"User with id {viewModel.Id} not found during POST request");
                        TempData["LogMessages"] = logMessages;
                        return NotFound();
                    }

                    userProfile.FirstName = viewModel.FirstName;
                    userProfile.LastName = viewModel.LastName;
                    userProfile.PhoneNumber = viewModel.PhoneNumber;
                    userProfile.Email = viewModel.Email;
                    userProfile.UserName = viewModel.Email;
                    userProfile.NormalizedUserName = viewModel.Email;
                    userProfile.NormalizedEmail = viewModel.Email;
                    userProfile.ModifiedBy = user.Id;
                    userProfile.ModifiedDateTime = DateTime.Now;

                    if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ProfilePictureFile);
                        userProfile.ProfilePicture = uploadedImagePath;
                    }

                    _context.Update(userProfile);
                    await _context.SaveChangesAsync();
                    logMessages.Add($"User with id {viewModel.Id} successfully updated");

                    var subject = "Profile Update Notification";
                    var emailBodyTemplate = $@"
                Dear {viewModel.FirstName} {viewModel.LastName},<br/><br/>
                Your profile information has been successfully updated.<br/><br/>
                If you did not request this update or if you have any questions, please contact us immediately.<br/><br/>
                Regards,<br/>
                Diski360 Management
            ";
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(viewModel.Email, subject, emailBodyTemplate, "Diski 360"));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logMessages.Add($"DbUpdateConcurrencyException occurred: {ex.Message}");
                    TempData["LogMessages"] = logMessages;
                    throw;
                }

                TempData["LogMessages"] = logMessages;

                await _requestLogService.LogSuceededRequest("Profile information updated successfully.", StatusCodes.Status200OK);


                if (User.IsInRole("Personnel Administrator"))
                {
                    TempData["Message"] = $"{userProfile.FirstName} {userProfile.LastName}'s information has been updated successfully.";
                    await _activityLogger.Log($"Updated {userProfile.FirstName} {userProfile.LastName} profile information", user.Id);
                }
                else
                {
                    TempData["Message"] = $"Your profile information has been updated successfully.";
                    await _activityLogger.Log($"Updated profile information", user.Id);
                }

                if (userRole == "Club Administrator")
                {
                    return RedirectToAction(nameof(ClubAdministrators));
                }
                else if (userRole == "Player")
                {
                    return RedirectToAction(nameof(DivisionPlayers));
                }
                else if (userRole == "Sport Administrator")
                {
                    return RedirectToAction(nameof(SportAdministrators));
                }
                else if (userRole == "Division Manager")
                {
                    return RedirectToAction(nameof(SportManagers));
                }
                else if (userRole == "Sport Coordinator")
                {
                    return RedirectToAction(nameof(SportCoordinators));
                }
                else if (userRole == "Official")
                {
                    return RedirectToAction(nameof(Officials));
                }
                else if (userRole == "Club Manager")
                {
                    return RedirectToAction(nameof(ClubManagers));
                }
                else if (userRole == "News Administrator")
                {
                    return RedirectToAction(nameof(NewsAdministrators));
                }
                else if (userRole == "News Updator")
                {
                    return RedirectToAction(nameof(NewsUpdaters));
                }
                else if (userRole == "Fans Administrator")
                {
                    return RedirectToAction(nameof(FansAdministrators));
                }
                else if (userRole == "System Administrator")
                {
                    if(User.IsInRole("Personnel Administrator"))
                    {
                        return RedirectToAction(nameof(SystemAdministrators));
                    }
                    else if(User.IsInRole("System Administrator"))
                    {
                        var encryptedUserId = _encryptionService.Encrypt(viewModel.Id);
                        return RedirectToAction(nameof(UserProfile), new { userId = encryptedUserId });
                    }
                }
                else
                {
                    if (User.IsInRole("Fans Administrator"))
                    {
                        return RedirectToAction(nameof(DivisionFans));
                    }
                    else if (userRole == "Personnel Administrator" || 
                        userRole == "System Administrator" || 
                        userRole == "Division Manager")
                    {
                        TempData["Message"] = $"Your profile information has been updated successfully.";
                        await _activityLogger.Log($"Updated profile information", user.Id);

                        var encryptedUserId = _encryptionService.Encrypt(viewModel.Id);
                        return RedirectToAction(nameof(UserProfile), new { userId = encryptedUserId });
                    }
                    else
                    {
                        var encryptedUserId = _encryptionService.Encrypt(viewModel.Id);
                        return RedirectToAction(nameof(UserProfile), new { userId = encryptedUserId });
                    }
                }
            }

            logMessages.Add($"Model state invalid for User with id {viewModel.Id}");

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    var errorMessage = $"Error in {state.Key}: {error.ErrorMessage}";
                    logMessages.Add(errorMessage);
                }
            }

            viewModel.ProfilePicture = _context.ClubAdministrator.Find(viewModel.Id)?.ProfilePicture;

            TempData["LogMessages"] = logMessages;
            viewModel.ProfilePicture = userProfile.ProfilePicture;
            return View(viewModel);
        }

        private bool ClubAdministratorExists(string userId)
        {
            return _context.Player.Any(e => e.Id == userId);
        }

        private bool ValidateProfileUpdatedProperties(UpdateProfileViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.FirstName, new ValidationContext(viewModel, null, null) { MemberName = "FirstName" }, validationResults);
            Validator.TryValidateProperty(viewModel.LastName, new ValidationContext(viewModel, null, null) { MemberName = "LastName" }, validationResults);
            Validator.TryValidateProperty(viewModel.PhoneNumber, new ValidationContext(viewModel, null, null) { MemberName = "PhoneNumber" }, validationResults);
            Validator.TryValidateProperty(viewModel.Email, new ValidationContext(viewModel, null, null) { MemberName = "Email" }, validationResults);
            return validationResults.Count == 0;
        }

        [Authorize(Roles = "Fans Administrator, Personnel Administrator, System Administrator")]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var loggedInUser = await _userManager.GetUserAsync(User);

            var user = await _context.UserBaseModel
                .Where(u => u.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == decryptedUserId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            user.IsActive = false;
            user.ModifiedBy = loggedInUser.Id;
            user.ModifiedDateTime = DateTime.Now;

            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Deactivated {user.FirstName} {user.LastName} profile.", user.Id);

            TempData["Message"] = $"You have deactivated {user.FirstName} {user.LastName} and they won't be able to use some system features until you activate them.";
            await _requestLogService.LogSuceededRequest("User account deactivated successfully.", StatusCodes.Status200OK);

            var subject = "Account Deactivation Notification";
            var emailBodyTemplate = $@"
        Dear {user.FirstName} {user.LastName},<br/><br/>
        Your account has been deactivated and you will not be able to use some system features until it is reactivated.<br/><br/>
        If you believe this is a mistake or if you have any questions, please contact us immediately.<br/><br/>
        Regards,<br/>
        Diski360 Management
            ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, subject, emailBodyTemplate, "Diski 360"));

            if(User.IsInRole("Club Administrator") 
                || User.IsInRole("Fans Administrator") 
                || User.IsInRole("Division Manager"))
            {
                if (userRole == "Club Administrator")
                {
                    return RedirectToAction(nameof(ClubAdministrators));
                }
                else if (userRole == "Player")
                {
                    return RedirectToAction(nameof(DivisionPlayers));
                }
                else if (userRole == "Sport Administrator")
                {
                    return RedirectToAction(nameof(SportAdministrators));
                }
                else if (userRole == "Division Manager")
                {
                    return RedirectToAction(nameof(SportManagers));
                }
                else if (userRole == "Sport Coordinator")
                {
                    return RedirectToAction(nameof(SportCoordinators));
                }
                else if (userRole == "Official")
                {
                    return RedirectToAction(nameof(Officials));
                }
                else if (userRole == "Club Manager")
                {
                    return RedirectToAction(nameof(ClubManagers));
                }
                else if (userRole == "News Administrator")
                {
                    return RedirectToAction(nameof(NewsAdministrators));
                }
                else if (userRole == "News Updator")
                {
                    return RedirectToAction(nameof(NewsUpdaters));
                }
                else if (userRole == "Fans Administrator")
                {
                    return RedirectToAction(nameof(FansAdministrators));
                }
                else if (userRole == "System Administrator")
                {
                    return RedirectToAction(nameof(SystemAdministrators));
                }
                else
                {
                    return RedirectToAction(nameof(DivisionFans));
                }
            }
            else if (User.IsInRole("System Administrator"))
            {
                return RedirectToAction(nameof(NetUsers));
            };

            return View();
           
        }

        [Authorize(Roles = "Fans Administrator, Personnel Administrator, System Administrator")]
        public async Task<IActionResult> Activate(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var loggedInUser = await _userManager.GetUserAsync(User);

            var user = await _context.UserBaseModel
                .Where(u => u.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == decryptedUserId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            user.IsActive = true;
            user.ModifiedBy = loggedInUser.Id;
            user.ModifiedDateTime = DateTime.Now;

            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Activated {user.FirstName} {user.LastName} profile.", user.Id);

            TempData["Message"] = $"You have successfully activated {user.FirstName} {user.LastName} and now they have access to the system features.";
            await _requestLogService.LogSuceededRequest("User account activated successfully.", StatusCodes.Status200OK);


            var subject = "Account Activation Notification";
            var emailBodyTemplate = $@"
        Dear {user.FirstName} {user.LastName},<br/><br/>
        Your account has been successfully activated, and you now have access to the system features.<br/><br/>
        If you have any questions or need assistance, please contact us immediately.<br/><br/>
        Regards,<br/>
        Diski360 Management
    ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, subject, emailBodyTemplate, "Diski 360"));

            if (User.IsInRole("Club Administrator")
      || User.IsInRole("Fans Administrator")
      || User.IsInRole("Division Manager"))
            {
                if (userRole == "Club Administrator")
                {
                    return RedirectToAction(nameof(ClubAdministrators));
                }
                else if (userRole == "Player")
                {
                    return RedirectToAction(nameof(DivisionPlayers));
                }
                else if (userRole == "Sport Administrator")
                {
                    return RedirectToAction(nameof(SportAdministrators));
                }
                else if (userRole == "Division Manager")
                {
                    return RedirectToAction(nameof(SportManagers));
                }
                else if (userRole == "Sport Coordinator")
                {
                    return RedirectToAction(nameof(SportCoordinators));
                }
                else if (userRole == "Official")
                {
                    return RedirectToAction(nameof(Officials));
                }
                else if (userRole == "Club Manager")
                {
                    return RedirectToAction(nameof(ClubManagers));
                }
                else if (userRole == "News Administrator")
                {
                    return RedirectToAction(nameof(NewsAdministrators));
                }
                else if (userRole == "News Updator")
                {
                    return RedirectToAction(nameof(NewsUpdaters));
                }
                else if (userRole == "Fans Administrator")
                {
                    return RedirectToAction(nameof(FansAdministrators));
                }
                else if (userRole == "System Administrator")
                {
                    return RedirectToAction(nameof(SystemAdministrators));
                }
                else
                {
                    return RedirectToAction(nameof(DivisionFans));
                }
            }
            else if (User.IsInRole("System Administrator"))
            {
                return RedirectToAction(nameof(NetUsers));
            };

            return View();
        }

        [Authorize(Roles = "Fans Administrator, Personnel Administrator, System Administrator")]
        public async Task<IActionResult> Suspend(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var loggedInUser = await _userManager.GetUserAsync(User);

            var user = await _context.UserBaseModel
                .Where(u => u.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == decryptedUserId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            user.IsSuspended = true;
            user.ModifiedBy = loggedInUser.Id;
            user.ModifiedDateTime = DateTime.Now;

            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Suspended {user.FirstName} {user.LastName} profile.", loggedInUser.Id);

            TempData["Message"] = $"You have suspended {user.FirstName} {user.LastName} and they won't be able to use the entire system until you unsuspend them.";
            await _requestLogService.LogSuceededRequest("User account suspended successfully.", StatusCodes.Status200OK);


            var subject = "Account Suspension Notification";
            var emailBodyTemplate = $@"
        Dear {user.FirstName} {user.LastName},<br/><br/>
        Your account has been suspended and you will not be able to access the system features until further notice.<br/><br/>
        If you believe this is a mistake or if you need assistance, please contact support immediately.<br/><br/>
        Regards,<br/>
        Diski360 Management
    ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, subject, emailBodyTemplate, "Diski 360"));

            if (User.IsInRole("Club Administrator")
     || User.IsInRole("Fans Administrator")
     || User.IsInRole("Division Manager"))
            {
                if (userRole == "Club Administrator")
                {
                    return RedirectToAction(nameof(ClubAdministrators));
                }
                else if (userRole == "Player")
                {
                    return RedirectToAction(nameof(DivisionPlayers));
                }
                else if (userRole == "Sport Administrator")
                {
                    return RedirectToAction(nameof(SportAdministrators));
                }
                else if (userRole == "Division Manager")
                {
                    return RedirectToAction(nameof(SportManagers));
                }
                else if (userRole == "Sport Coordinator")
                {
                    return RedirectToAction(nameof(SportCoordinators));
                }
                else if (userRole == "Official")
                {
                    return RedirectToAction(nameof(Officials));
                }
                else if (userRole == "Club Manager")
                {
                    return RedirectToAction(nameof(ClubManagers));
                }
                else if (userRole == "News Administrator")
                {
                    return RedirectToAction(nameof(NewsAdministrators));
                }
                else if (userRole == "News Updator")
                {
                    return RedirectToAction(nameof(NewsUpdaters));
                }
                else if (userRole == "Fans Administrator")
                {
                    return RedirectToAction(nameof(FansAdministrators));
                }
                else if (userRole == "System Administrator")
                {
                    return RedirectToAction(nameof(SystemAdministrators));
                }
                else
                {
                    return RedirectToAction(nameof(DivisionFans));
                }
            }
            else if (User.IsInRole("System Administrator"))
            {
                return RedirectToAction(nameof(NetUsers));
            };

            return View();
        }


        [Authorize(Roles = "Fans Administrator, Personnel Administrator, System Administrator")]
        public async Task<IActionResult> Unsuspend(string userId)
        {

            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var loggedInUser = await _userManager.GetUserAsync(User);

            var user = await _context.UserBaseModel
                .Where(u => u.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == decryptedUserId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            user.IsSuspended = false;
            user.ModifiedBy = loggedInUser.Id;
            user.ModifiedDateTime = DateTime.Now;

            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Unsuspended {user.FirstName} {user.LastName} profile.", loggedInUser.Id);

            TempData["Message"] = $"You have successfully unsuspended {user.FirstName} {user.LastName} and now they have access to the features of this system.";
            await _requestLogService.LogSuceededRequest("User account unsuspended successfully.", StatusCodes.Status200OK);


            var subject = "Account Unsuspension Notification";
            var emailBodyTemplate = $@"
        Dear {user.FirstName} {user.LastName},<br/><br/>
        Your account has been successfully unsuspended, and you now have access to the system features.<br/><br/>
        If you have any questions or need further assistance, please contact support.<br/><br/>
        Regards,<br/>
        Diski360 Management
    ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, subject, emailBodyTemplate, "Diski 360"));

            if (User.IsInRole("Club Administrator")
      || User.IsInRole("Fans Administrator")
      || User.IsInRole("Division Manager"))
            {
                if (userRole == "Club Administrator")
                {
                    return RedirectToAction(nameof(ClubAdministrators));
                }
                else if (userRole == "Player")
                {
                    return RedirectToAction(nameof(DivisionPlayers));
                }
                else if (userRole == "Sport Administrator")
                {
                    return RedirectToAction(nameof(SportAdministrators));
                }
                else if (userRole == "Division Manager")
                {
                    return RedirectToAction(nameof(SportManagers));
                }
                else if (userRole == "Sport Coordinator")
                {
                    return RedirectToAction(nameof(SportCoordinators));
                }
                else if (userRole == "Official")
                {
                    return RedirectToAction(nameof(Officials));
                }
                else if (userRole == "Club Manager")
                {
                    return RedirectToAction(nameof(ClubManagers));
                }
                else if (userRole == "News Administrator")
                {
                    return RedirectToAction(nameof(NewsAdministrators));
                }
                else if (userRole == "News Updator")
                {
                    return RedirectToAction(nameof(NewsUpdaters));
                }
                else if (userRole == "Fans Administrator")
                {
                    return RedirectToAction(nameof(FansAdministrators));
                }
                else if (userRole == "System Administrator")
                {
                    return RedirectToAction(nameof(SystemAdministrators));
                }
                else
                {
                    return RedirectToAction(nameof(DivisionFans));
                }
            }
            else if (User.IsInRole("System Administrator"))
            {
                return RedirectToAction(nameof(NetUsers));
            };

            return View();
        }

        [Authorize(Roles = "Fans Administrator, Personnel Administrator, System Administrator")]
        public async Task<IActionResult> Delete(string userId)
        {

            var decryptedUserId = _encryptionService.Decrypt(userId);


            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var loggedInUser = await _userManager.GetUserAsync(User);

            var existingUser = await _context.UserBaseModel
                .Where(e => e.Id == decryptedUserId)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Where(ur => ur.UserId == decryptedUserId)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            existingUser.IsDeleted = true;
            existingUser.ModifiedBy = loggedInUser.Id;
            existingUser.ModifiedDateTime = DateTime.Now;

            _context.Update(existingUser);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Deleted {existingUser.FirstName} {existingUser.LastName} profile from the system.", loggedInUser.Id);

            TempData["Message"] = $"You have deleted {existingUser.FirstName} {existingUser.LastName} and they no longer exist in this system.";
            await _requestLogService.LogSuceededRequest("User account deleted successfully.", StatusCodes.Status200OK);

            var subject = "Account Deletion Confirmation";
            var emailBodyTemplate = $@"
        Dear {existingUser.FirstName} {existingUser.LastName},<br/><br/>
        We want to inform you that your account has been permanently deleted from our system. You will no longer have access to any of its features.<br/><br/>
        If you believe this action was taken in error or if you have any questions, please contact our support team.<br/><br/>
        Regards,<br/>
        Diski360 Management
    ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(existingUser.Email, subject, emailBodyTemplate, "Diski 360"));

            if (User.IsInRole("Club Administrator")
     || User.IsInRole("Fans Administrator")
     || User.IsInRole("Division Manager"))
            {
                if (userRole == "Club Administrator")
                {
                    return RedirectToAction(nameof(ClubAdministrators));
                }
                else if (userRole == "Player")
                {
                    return RedirectToAction(nameof(DivisionPlayers));
                }
                else if (userRole == "Sport Administrator")
                {
                    return RedirectToAction(nameof(SportAdministrators));
                }
                else if (userRole == "Division Manager")
                {
                    return RedirectToAction(nameof(SportManagers));
                }
                else if (userRole == "Sport Coordinator")
                {
                    return RedirectToAction(nameof(SportCoordinators));
                }
                else if (userRole == "Official")
                {
                    return RedirectToAction(nameof(Officials));
                }
                else if (userRole == "Club Manager")
                {
                    return RedirectToAction(nameof(ClubManagers));
                }
                else if (userRole == "News Administrator")
                {
                    return RedirectToAction(nameof(NewsAdministrators));
                }
                else if (userRole == "News Updator")
                {
                    return RedirectToAction(nameof(NewsUpdaters));
                }
                else if (userRole == "Fans Administrator")
                {
                    return RedirectToAction(nameof(FansAdministrators));
                }
                else if (userRole == "System Administrator")
                {
                    return RedirectToAction(nameof(SystemAdministrators));
                }
                else
                {
                    return RedirectToAction(nameof(DivisionFans));
                }
            }
            else if (User.IsInRole("System Administrator"))
            {
                return RedirectToAction(nameof(NetUsers));
            };

            return View();
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> ChangeUserRole(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);
            return View();
        }


        /*Update all users methods*/
        [Authorize(Roles = ("Club Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdateClubManager(string userId)
        {
            var logMessages = new List<string>();

            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null || _context.ClubManager == null)
            {
                logMessages.Add($"UpdateClubManager GET request failed: id is null or ClubManager context is null");
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var clubManager = await _context.ClubManager.FindAsync(decryptedUserId);
            if (clubManager == null)
            {
                logMessages.Add($"ClubManager with id {decryptedUserId} not found during GET request");
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var clubManagerViewModel = new UpdateClubManagerViewModel
            {
                Id = decryptedUserId,
                FirstName = clubManager.FirstName,
                LastName = clubManager.LastName,
                PhoneNumber = clubManager.PhoneNumber,
                ProfilePicture = clubManager.ProfilePicture,
                Email = clubManager.Email,
            };

            TempData["LogMessages"] = logMessages;
            return View(clubManagerViewModel);
        }


        [Authorize(Roles = ("Club Administrator"))]
        [HttpPost]
        public async Task<IActionResult> UpdateClubManager(UpdateClubManagerViewModel viewModel, IFormFile ProfilePictureFile)
        {
            var logMessages = new List<string>();

            if (viewModel.Id == null)
            {
                logMessages.Add($"UpdateClubManager POST request failed: id mismatch: {viewModel.Id} != {viewModel.Id}");
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var clubManager = await _context.ClubManager.FindAsync(viewModel.Id);

            if (ValidateUpdatedProperties(viewModel))
            {
                try
                {
                    if (clubManager == null)
                    {
                        logMessages.Add($"ClubManager with id {viewModel.Id} not found during POST request");
                        TempData["LogMessages"] = logMessages;
                        return NotFound();
                    }

                    clubManager.FirstName = viewModel.FirstName;
                    clubManager.LastName = viewModel.LastName;
                    clubManager.PhoneNumber = viewModel.PhoneNumber;
                    clubManager.Email = viewModel.Email;
                    clubManager.UserName = viewModel.Email;
                    clubManager.NormalizedEmail = viewModel.Email;
                    clubManager.NormalizedUserName = viewModel.Email;
                    clubManager.ModifiedBy = user.Id;
                    clubManager.ModifiedDateTime = DateTime.Now;

                    if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ProfilePictureFile);
                        clubManager.ProfilePicture = uploadedImagePath;
                    }

                    _context.Update(clubManager);
                    await _context.SaveChangesAsync();
                    logMessages.Add($"ClubManager with id {viewModel.Id} successfully updated");

                    var subject = "Your Profile Has Been Updated";
                    var emailBodyTemplate = $@"
                Dear {clubManager.FirstName} {clubManager.LastName},<br/><br/>
                Your profile information has been successfully updated. If you did not make this change, please contact our support team immediately.<br/><br/>
                Regards,<br/>
                Diski360 Management
            ";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(clubManager.Email, subject, emailBodyTemplate, "Diski 360"));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logMessages.Add($"DbUpdateConcurrencyException occurred: {ex.Message}");
                    TempData["LogMessages"] = logMessages;
                    throw;
                }

                await _activityLogger.Log($"Updated {clubManager.FirstName} {clubManager.LastName} profile.", user.Id);
                await _requestLogService.LogSuceededRequest("Club manager profile information updated successfully.", StatusCodes.Status200OK);

                TempData["Message"] = $"{clubManager.FirstName} {clubManager.LastName} information has been updated successfully.";
                return RedirectToAction(nameof(MyClubManagers));
            }

            logMessages.Add($"Model state invalid for ClubManager with id {viewModel.Id}");

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    var errorMessage = $"Error in {state.Key}: {error.ErrorMessage}";
                    logMessages.Add(errorMessage);
                }
            }

            viewModel.ProfilePicture = _context.ClubManager.Find(viewModel.Id)?.ProfilePicture;

            TempData["LogMessages"] = logMessages;
            viewModel.ProfilePicture = clubManager.ProfilePicture;
            return View(viewModel);
        }

        private bool ValidateUpdatedProperties(UpdateClubManagerViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.FirstName, new ValidationContext(viewModel, null, null) { MemberName = "FirstName" }, validationResults);
            Validator.TryValidateProperty(viewModel.LastName, new ValidationContext(viewModel, null, null) { MemberName = "LastName" }, validationResults);
            Validator.TryValidateProperty(viewModel.PhoneNumber, new ValidationContext(viewModel, null, null) { MemberName = "PhoneNumber" }, validationResults);
            Validator.TryValidateProperty(viewModel.Email, new ValidationContext(viewModel, null, null) { MemberName = "Email" }, validationResults);
            return validationResults.Count == 0;
        }


        [Authorize(Roles = ("Club Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdatePlayerProfile(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (string.IsNullOrEmpty(decryptedUserId))
            {
                return NotFound();
            }

            var clubPlayer = await _context.Player.FindAsync(decryptedUserId);
            if (clubPlayer == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateClubPlayerViewModel
            {
                Id = decryptedUserId,
                FirstName = clubPlayer.FirstName,
                LastName = clubPlayer.LastName,
                PhoneNumber = clubPlayer.PhoneNumber,
                Email = clubPlayer.Email,
                Position = clubPlayer.Position,
                JerseyNumber = clubPlayer.JerseyNumber,
                ProfilePicture = clubPlayer.ProfilePicture,
                MarketValue = clubPlayer.MarketValue,
                PlayerCard = clubPlayer.PlayerCard
            };

            ViewBag.Positions = Enum.GetValues(typeof(Position))
                                     .Cast<Position>()
                                     .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            return View(viewModel);
        }

        [Authorize(Roles = ("Club Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePlayerProfile(UpdateClubPlayerViewModel viewModel)
        {
            if (viewModel.Id == null)
            {
                return NotFound();
            }

            if (ValidateUpdatedProperties(viewModel))
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);

                    var clubPlayer = await _context.Player.FindAsync(viewModel.Id);
                    if (clubPlayer == null)
                    {
                        return NotFound();
                    }

                    clubPlayer.FirstName = viewModel.FirstName;
                    clubPlayer.LastName = viewModel.LastName;
                    clubPlayer.PhoneNumber = viewModel.PhoneNumber;
                    clubPlayer.Email = viewModel.Email;
                    clubPlayer.Position = viewModel.Position;
                    clubPlayer.JerseyNumber = viewModel.JerseyNumber;
                    clubPlayer.UserName = viewModel.Email;
                    clubPlayer.NormalizedEmail = viewModel.Email;
                    clubPlayer.NormalizedUserName = viewModel.Email;
                    clubPlayer.ModifiedBy = user.Id;
                    clubPlayer.ModifiedDateTime = DateTime.Now;
                    clubPlayer.MarketValue = viewModel.MarketValue;

                    if (viewModel.ProfilePictureFile != null && viewModel.ProfilePictureFile.Length > 0)
                    {
                        var playerProfilePicturePath = await _fileUploadService.UploadFileAsync(viewModel.ProfilePictureFile);
                        clubPlayer.ProfilePicture = playerProfilePicturePath;
                    }

                    if (viewModel.PlayerCardFile != null && viewModel.PlayerCardFile.Length > 0)
                    {
                        var playerCardPath = await _fileUploadService.UploadFileAsync(viewModel.PlayerCardFile);
                        clubPlayer.PlayerCard = playerCardPath;
                    }

                    _context.Update(clubPlayer);
                    await _context.SaveChangesAsync();

                    var subject = "Your Profile Has Been Updated";
                    var emailBodyTemplate = $@"
                Dear {clubPlayer.FirstName} {clubPlayer.LastName},<br/><br/>
                Your profile information has been successfully updated. If you did not make this change, please contact our support team immediately.<br/><br/>
                Regards,<br/>
                Diski360 Management
            ";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(clubPlayer.Email, subject, emailBodyTemplate, "Diski 360"));

                    await _activityLogger.Log($"Updated {clubPlayer.FirstName} {clubPlayer.LastName} profile.", user.Id);

                    TempData["Message"] = $"{clubPlayer.FirstName} {clubPlayer.LastName} information has been updated successfully.";
                    await _requestLogService.LogSuceededRequest("Player profile information updated successfully.", StatusCodes.Status200OK);

                    return RedirectToAction("MyClubPlayers");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubPlayerExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Positions = Enum.GetValues(typeof(Position))
                                    .Cast<Position>()
                                    .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            return View(viewModel);
        }

        private bool ValidateUpdatedProperties(UpdateClubPlayerViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.FirstName, new ValidationContext(viewModel, null, null) { MemberName = "FirstName" }, validationResults);
            Validator.TryValidateProperty(viewModel.LastName, new ValidationContext(viewModel, null, null) { MemberName = "LastName" }, validationResults);
            Validator.TryValidateProperty(viewModel.PhoneNumber, new ValidationContext(viewModel, null, null) { MemberName = "PhoneNumber" }, validationResults);
            Validator.TryValidateProperty(viewModel.Email, new ValidationContext(viewModel, null, null) { MemberName = "Email" }, validationResults);
            Validator.TryValidateProperty(viewModel.Position, new ValidationContext(viewModel, null, null) { MemberName = "Position" }, validationResults);
            Validator.TryValidateProperty(viewModel.JerseyNumber, new ValidationContext(viewModel, null, null) { MemberName = "JerseyNumber" }, validationResults);
            Validator.TryValidateProperty(viewModel.MarketValue, new ValidationContext(viewModel, null, null) { MemberName = "MarketValue" }, validationResults);
            return validationResults.Count == 0;
        }

        private bool ClubPlayerExists(string userId)
        {
            return _context.Player.Any(e => e.Id == userId);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdateClubAdministrator(string userId)
        {
            var logMessages = new List<string>();

            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null || _context.ClubAdministrator == null)
            {
                logMessages.Add($"UpdateClubManager GET request failed: id is null or ClubManager context is null");
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var clubAdministrator = await _context.ClubAdministrator.FindAsync(decryptedUserId);
            if (clubAdministrator == null)
            {
                logMessages.Add($"ClubManager with id {decryptedUserId} not found during GET request");
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var clubManagerViewModel = new UpdateClubAdministratorViewModel
            {
                Id = decryptedUserId,
                FirstName = clubAdministrator.FirstName,
                LastName = clubAdministrator.LastName,
                PhoneNumber = clubAdministrator.PhoneNumber,
                ProfilePicture = clubAdministrator.ProfilePicture,
                Email = clubAdministrator.Email,
            };

            TempData["LogMessages"] = logMessages;
            return View(clubManagerViewModel);
        }



        [Authorize(Roles = ("Personnel Administrator"))]
        [HttpPost]
        public async Task<IActionResult> UpdateClubAdministrator(UpdateClubAdministratorViewModel viewModel, IFormFile ProfilePictureFile)
        {
            var logMessages = new List<string>();

            if (viewModel.Id == null)
            {
                logMessages.Add($"UpdateClubAdministrator POST request failed: id mismatch: {viewModel.Id} != {viewModel.Id}");
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var clubAdministrator = await _context.ClubAdministrator.FindAsync(viewModel.Id);

            if (ValidateUpdatedProperties(viewModel))
            {
                try
                {
                    if (clubAdministrator == null)
                    {
                        logMessages.Add($"ClubAdministrator with id {viewModel.Id} not found during POST request");
                        TempData["LogMessages"] = logMessages;
                        return NotFound();
                    }

                    clubAdministrator.FirstName = viewModel.FirstName;
                    clubAdministrator.LastName = viewModel.LastName;
                    clubAdministrator.PhoneNumber = viewModel.PhoneNumber;
                    clubAdministrator.Email = viewModel.Email;
                    clubAdministrator.UserName = viewModel.Email;
                    clubAdministrator.NormalizedUserName = viewModel.Email;
                    clubAdministrator.NormalizedEmail = viewModel.Email;
                    clubAdministrator.ModifiedBy = user.Id;
                    clubAdministrator.ModifiedDateTime = DateTime.Now;

                    if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ProfilePictureFile);
                        clubAdministrator.ProfilePicture = uploadedImagePath;
                    }

                    _context.Update(clubAdministrator);
                    await _context.SaveChangesAsync();
                    logMessages.Add($"ClubAdministrator with id {viewModel.Id} successfully updated");

                    var subject = "Your Profile Has Been Updated";
                    var emailBodyTemplate = $@"
                Dear {clubAdministrator.FirstName} {clubAdministrator.LastName},<br/><br/>
                Your profile information has been successfully updated. If you did not make this change, please contact our support team immediately.<br/><br/>
                Regards,<br/>
                Diski360 Management
                    ";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(clubAdministrator.Email, subject, emailBodyTemplate, "Diski 360"));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logMessages.Add($"DbUpdateConcurrencyException occurred: {ex.Message}");
                    TempData["LogMessages"] = logMessages;
                    throw;
                }

                TempData["LogMessages"] = logMessages;
                await _activityLogger.Log($"Updated {clubAdministrator.FirstName} {clubAdministrator.LastName} profile.", user.Id);
                await _requestLogService.LogSuceededRequest("Club administrator profile information updated successfully.", StatusCodes.Status200OK);

                TempData["Message"] = $"{clubAdministrator.FirstName} {clubAdministrator.LastName} information has been updated successfully.";
                return RedirectToAction(nameof(ClubAdministrators));
            }

            logMessages.Add($"Model state invalid for ClubAdministrator with id {viewModel.Id}");

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    var errorMessage = $"Error in {state.Key}: {error.ErrorMessage}";
                    logMessages.Add(errorMessage);
                }
            }

            viewModel.ProfilePicture = _context.ClubAdministrator.Find(viewModel.Id)?.ProfilePicture;

            TempData["LogMessages"] = logMessages;
            viewModel.ProfilePicture = clubAdministrator.ProfilePicture;
            return View(viewModel);
        }


        private bool ValidateUpdatedProperties(UpdateClubAdministratorViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.FirstName, new ValidationContext(viewModel, null, null) { MemberName = "FirstName" }, validationResults);
            Validator.TryValidateProperty(viewModel.LastName, new ValidationContext(viewModel, null, null) { MemberName = "LastName" }, validationResults);
            Validator.TryValidateProperty(viewModel.PhoneNumber, new ValidationContext(viewModel, null, null) { MemberName = "PhoneNumber" }, validationResults);
            Validator.TryValidateProperty(viewModel.Email, new ValidationContext(viewModel, null, null) { MemberName = "Email" }, validationResults);
            return validationResults.Count == 0;
        }


        [Authorize(Roles = ("Club Administrator"))]
        [HttpGet]
        public async Task<IActionResult> EndClubManagerContract(string userId)
        {
            var decryptedUserId = _encryptionService.Decrypt(userId);

            if (decryptedUserId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubManager = await _context.ClubManager
                .Where(c => c.Id == decryptedUserId && c.ClubId == clubAdministrator.ClubId)
                .FirstOrDefaultAsync();

            var viewModel = new EndClubManagerContractViewModel
            {
                FullNames = $"{clubManager.FirstName} {clubManager.LastName}",
                ProfilePicture = clubManager.ProfilePicture,
                PhoneNumber = clubManager.PhoneNumber,
                Email = clubManager.Email,
                UserId = decryptedUserId
            };

            return View(viewModel);
        }

        [Authorize(Roles = ("Club Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndClubManagerContract(EndClubManagerContractViewModel viewModel)
        {
            if (viewModel.UserId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

           

            var clubManager = await _context.ClubManager
                .Where(c => c.Id == viewModel.UserId && c.ClubId == clubAdministrator.ClubId)
                .Include(c => c.Club)
                .FirstOrDefaultAsync();

/*            if (clubManager.IsContractEnded == true)
            {
                TempData["Message"] = $"There is no contract binding you and {clubManager.FirstName} {clubManager.LastName}.";
                return RedirectToAction(nameof(MyClubManagers));
            }*/

                if (clubManager == null)
            {
                return NotFound();
            }

            clubManager.IsActive = false;
            clubManager.IsSuspended = true;
            clubManager.IsContractEnded = true;
            clubManager.IsDeleted = true;

            _context.Update(clubManager);
            await _context.SaveChangesAsync();

            var subject = "Notification of Contract Termination";
            var emailBodyTemplate = $@"
        <html>
        <body>
            <p>Dear {clubManager.FirstName} {clubManager.LastName},</p>
            <p>We hope this message finds you well.</p>
            <p>We are writing to inform you that your contract with {clubManager.Club.ClubName} has been officially terminated, effective immediately. As of today, your association with the club has ended, and you will no longer have access to club resources or participate in club activities.</p>
            <p>We appreciate the contributions you have made during your tenure and regret that this decision had to be made. If you have any questions regarding this termination or need clarification on any matter, please do not hesitate to contact our HR department.</p>
            <p>Thank you for your understanding and cooperation.</p>
            <p>Best regards,</p>
            <p>The Diski360 Management Team</p>
            <p><strong>Note:</strong> Please ensure you have returned all club property and settled any outstanding matters before your departure.</p>
        </body>
        </html>
            ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(clubManager.Email, subject, emailBodyTemplate, "Diski 360"));

            TempData["Message"] = $"You have ended {viewModel.FullNames}'s contract with {clubManager.Club.ClubName}.";
            await _requestLogService.LogSuceededRequest("Manager contract ended successfully.", StatusCodes.Status200OK);

            return RedirectToAction(nameof(MyClubManagers));
        }
    }
}
