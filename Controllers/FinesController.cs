using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;
using MyField.ViewModels;
using Microsoft.AspNetCore.Identity;
using MyField.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using MyField.Services;
using Microsoft.AspNetCore.Authorization;
using Hangfire;

namespace MyField.Controllers
{
    public class FinesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;


        public FinesController(Ksans_SportsDbContext context,
              UserManager<UserBaseModel> userManager,
              IActivityLogger activityLogger,
              EmailService emailService,
              IEncryptionService encryptionService,
              RequestLogService requestLogService)
        {
            _userManager = userManager;
            _context = context;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        public async Task<IActionResult> Details(string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            if (decryptedFineId == null)
            {
                return NotFound();
            }

            var fine = await _context.Fines
                .Where(f => f.FineId == decryptedFineId)
                .FirstOrDefaultAsync();

            var viewModel = new FineDetailsViewModel
            {
                FineId = decryptedFineId,
                FineDetails = fine.FineDetails,
                FineAmount = fine.FineAmount,
                FineDueDate = fine.FineDuDate,
                PaymentStatus = fine.PaymentStatus
            };

            return View(viewModel);
        }


        [Authorize(Roles =("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> _PendingClubFines()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var _pendingFines = await _context.Fines
                .Where(p => p.PaymentStatus == PaymentStatus.Pending && 
                p.Club != null&&
                p.DivisionId == divisionId)
                .Include(p => p.Club)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .OrderByDescending(p=> p.CreatedDateTime)
                .ToListAsync();

            return PartialView("_PendingClubFinesPartial", _pendingFines);
        }

        [Authorize(Roles = "Sport Administrator, Division Manager")]
        public async Task<IActionResult> PaidClubFines()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var _paidFines = await _context.Fines
                .Where(p => p.PaymentStatus == PaymentStatus.Paid && 
                p.Club != null &&
                p.DivisionId == divisionId)
                .Include(p => p.Club)
                .OrderByDescending(p => p.CreatedDateTime)
                .ToListAsync();

            return PartialView("_PaidClubFinesPartial", _paidFines);
        }


        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> OverdueClubFines()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var overdueFines = await _context.Fines
                .Where(p => p.PaymentStatus == PaymentStatus.Overdue && 
                p.Club != null && 
                p.DivisionId == divisionId)
                .Include(p => p.Club)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .OrderByDescending(p => p.CreatedDateTime)
                .ToListAsync();

            return PartialView("_OverdueClubFinesPartial", overdueFines);
        }

        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> PendingClubFines ()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var pendingFines = await _context.Fines
                .Where(p => p.PaymentStatus == PaymentStatus.Pending && 
                p.Club != null &&
                p.DivisionId == divisionId)
                .Include(p => p.Club)
                .Include(p => p.CreatedBy)
                .Include(p => p.ModifiedBy)
                .ToListAsync();

            return View(pendingFines);
        }

        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> ClubFines()
        {
            return View();
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> IndividualFines()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var ksans_SportsDbContext = _context.Fines
                .Where(mo => mo.OffenderId != null &&
                mo.DivisionId == divisionId) 
                .Include(f => f.Offender)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .ToListAsync();

            return View(await ksans_SportsDbContext);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyPendingClubFines()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var myPendingClubFines = await _context.Fines
                .Where(mo => mo.ClubId == clubAdministrator.ClubId && mo.PaymentStatus.Equals(PaymentStatus.Pending))
                .Include(s => s.Club)
                .ToListAsync();

            return PartialView("_MyPendingClubFinesPartial", myPendingClubFines);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyPaidClubFines()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var myPaidClubFines = await _context.Fines
                .Where(mo => mo.ClubId == clubAdministrator.ClubId && mo.PaymentStatus.Equals(PaymentStatus.Paid))
                .Include(s => s.Club)
                .ToListAsync();

            return PartialView("_MyPaidClubFinesPartial", myPaidClubFines);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyOverDueClubFines()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var myOverDueClubFines = await _context.Fines
                .Where(mo => mo.ClubId == clubAdministrator.ClubId && mo.PaymentStatus.Equals(PaymentStatus.Overdue))
                .Include(s => s.Club)
                .ToListAsync();

            return PartialView("_MyOverDueClubFinesPartial", myOverDueClubFines);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyClubFines()
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

        [Authorize]
        public async Task<IActionResult> MyIndividualFines()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyPendingFines()
        {

            var user = await _userManager.GetUserAsync(User);

            var myFines = await _context.Fines
                .Where(f => f.OffenderId == user.Id && f.PaymentStatus == PaymentStatus.Pending)
                .Include(f => f.Offender)
                .ToListAsync();

            return PartialView("_MyPendingFinesPartial", myFines);
        }

        [Authorize]
        public async Task<IActionResult> MyPaidFines()
        {
            var user = await _userManager.GetUserAsync(User);

            var myFines = await _context.Fines
                .Where(f => f.OffenderId == user.Id && f.PaymentStatus == PaymentStatus.Paid)
                .Include(f => f.Offender)
                .ToListAsync();

            return PartialView("_MyPaidFinesPartial", myFines);
        }

        [Authorize]
        public async Task<IActionResult> MyOverDueFines()
        {
            var user = await _userManager.GetUserAsync(User);

            var myFines = await _context.Fines
                .Where(f => f.OffenderId == user.Id && f.PaymentStatus == PaymentStatus.Overdue)
                .Include(f => f.Offender)
                .ToListAsync();

            return PartialView("_MyOverDueFinesPartial", myFines);
        }


        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> ClubFineDetails(int? id)
        {
            if (id == null || _context.Fines == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var fine = await _context.Fines
                .Where(f => f.DivisionId == divisionId)
                .Include(f => f.Club)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .FirstOrDefaultAsync(m => m.FineId == id);
            if (fine == null)
            {
                return NotFound();
            }

            return View(fine);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> IndividualFineDetails(int? id)
        {
            if (id == null || _context.Fines == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var fine = await _context.Fines
                .Where(f => f.DivisionId == divisionId)
                .Include(f => f.Club)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .FirstOrDefaultAsync(m => m.FineId == id);
            if (fine == null)
            {
                return NotFound();
            }

            return View(fine);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        [HttpGet]
        public async Task<IActionResult> CreateIndividualFine()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId ??
                             (user as SportsMember)?.DivisionId ??
                             (user as Officials)?.DivisionId ??
                             (user as DivisionManager)?.DivisionId;

            if (divisionId == null)
            {
                return Forbid(); 
            }

            var filteredUsers = _context.SystemUsers
                .Where(u =>
                    _context.Player.Any(p => p.Id == u.Id && p.DivisionId == divisionId) ||
                    _context.ClubAdministrator.Any(ca => ca.Id == u.Id && ca.DivisionId == divisionId) ||
                    _context.ClubManager.Any(cm => cm.Id == u.Id && cm.DivisionId == divisionId) ||
                    _context.Officials.Any(o => o.Id == u.Id && o.DivisionId == divisionId) ||
                    _context.SportMember.Any(sm => sm.Id == u.Id && sm.DivisionId == divisionId))
                .Select(u => new
                {
                    Id = u.Id,
                    FullNameWithRole = u.FirstName + " " + u.LastName + " (" +
                                       string.Join(", ", _context.UserRoles
                                           .Where(ur => ur.UserId == u.Id)
                                           .Join(_context.Roles,
                                                 ur => ur.RoleId,
                                                 r => r.Id,
                                                 (ur, r) => r.Name)) + ")" 
                });


            ViewBag.SystemUsers = new SelectList(filteredUsers, "Id", "FullNameWithRole");

            return View();
        }





        [Authorize(Roles = ("Personnel Administrator"))]
        [HttpPost]
        public async Task<IActionResult> CreateIndividualFine(CreateIndividualFineViewModel viewModel)
        {

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            if (ModelState.IsValid)
            {

                var personnelFinancialReport = await _context.PersonnelFinancialReports
                    .FirstOrDefaultAsync();

                var newIndividualFine = new Fine
                {
                    FineDetails = viewModel.FineDetails,
                    RuleViolated = viewModel.RuleViolated,
                    FineAmount = viewModel.FineAmount,
                    FineDuDate = viewModel.FineDuDate,
                    OffenderId = viewModel.OffenderId,
                    CreatedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = userId,
                    ModifiedDateTime = DateTime.Now,
                    PaymentStatus = PaymentStatus.Pending,
                    DivisionId = divisionId
                };

                if (personnelFinancialReport != null)
                {
                    personnelFinancialReport.ExpectedRepayableAmount += viewModel.FineAmount;
                    personnelFinancialReport.RepayableFinesCount++;
                }

                _context.Add(newIndividualFine);
                await _context.SaveChangesAsync();

                var newSavedFine = await _context.Fines
                    .Where(f => f.OffenderId == newIndividualFine.OffenderId)
                    .Include(f => f.Offender)
                    .FirstOrDefaultAsync();

                if (newSavedFine?.Offender != null)
                {
                    string offenderEmailBody = $@"
            Hi {newSavedFine.Offender.FirstName} {newSavedFine.Offender.LastName},<br/><br/>
            You have been fined for the following violation:<br/><br/>
            Rule Violated: {newSavedFine.RuleViolated}<br/>
            Fine Amount: {newSavedFine.FineAmount}<br/>
            Due Date: {newSavedFine.FineDuDate.ToShortDateString()}<br/><br/>
            Please ensure that you pay the fine by the due date. If you have any questions, contact our support team.<br/><br/>
            Kind regards,<br/>
            Diski360 Finance Team
            ";

                    BackgroundJob.Enqueue<EmailService>(service =>
                        service.SendEmailAsync(newSavedFine.Offender.Email, "Fine Notification", offenderEmailBody, "Diski 360"));
                }

                string userEmailBody = $@"
        Hi {user.FirstName} {user.LastName},<br/><br/>
        You have successfully filed a fine against {newSavedFine.Offender.FirstName} {newSavedFine.Offender.LastName}.<br/><br/>
        The offence requires a payment of {newSavedFine.FineAmount} before {newSavedFine.FineDuDate.ToShortDateString()}.<br/><br/>
        The email concerning this charge has been sent to the relevant individual.<br/><br/>
        Kind regards,<br/>
        Diski360 Finance Team
        ";

                BackgroundJob.Enqueue<EmailService>(service =>
                    service.SendEmailAsync(user.Email, "Fine Filed Successfully", userEmailBody, "Diski 360"));

                TempData["Message"] = $"You have successfully filed a fine/offence against {newSavedFine.Offender.FirstName} {newSavedFine.Offender.LastName}. The offence is required to pay an amount of {newSavedFine.FineAmount} before {newSavedFine.FineDuDate}. The email concerning this charge has been sent to the relevant individual.";
                await _activityLogger.Log($"Charged {newSavedFine.Offender.FirstName} {newSavedFine.Offender.LastName} R{newSavedFine.FineAmount} for {newSavedFine.RuleViolated}", user.Id);

                await _requestLogService.LogSuceededRequest("Successfully filed an individual fine", StatusCodes.Status200OK);

                return RedirectToAction(nameof(IndividualFines));
            }

            var filteredUsers = _context.SystemUsers
                .Where(u =>
                    _context.Player.Any(p => p.Id == u.Id && p.DivisionId == divisionId) ||
                    _context.ClubAdministrator.Any(ca => ca.Id == u.Id && ca.DivisionId == divisionId) ||
                    _context.ClubManager.Any(cm => cm.Id == u.Id && cm.DivisionId == divisionId) ||
                    _context.Officials.Any(o => o.Id == u.Id && o.DivisionId == divisionId) ||
                    _context.SportMember.Any(sm => sm.Id == u.Id && sm.DivisionId == divisionId))
                .Select(u => new
                {
                    Id = u.Id,
                    FullNameWithRole = u.FirstName + " " + u.LastName + " (" +
                                       string.Join(", ", _context.UserRoles
                                           .Where(ur => ur.UserId == u.Id)
                                           .Join(_context.Roles,
                                                 ur => ur.RoleId,
                                                 r => r.Id,
                                                 (ur, r) => r.Name)) + ")"
                });

            ViewBag.SystemUsers = new SelectList(filteredUsers, "Id", "FullName");

            await _requestLogService.LogFailedRequest("Failed to file an individual fine", StatusCodes.Status500InternalServerError);

            return View(viewModel);
        }


        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> CreateClubFine()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId ??
                             (user as SportsMember)?.DivisionId ??
                             (user as Officials)?.DivisionId ??
                             (user as DivisionManager)?.DivisionId;

            if (divisionId == null)
            {
                return Forbid();
            }

            var clubs = _context.Club
                .Where(c => c.DivisionId == divisionId)
                .Select(c => new
                {
                    ClubId = c.ClubId,
                    ClubName = c.ClubName
                });

            ViewBag.Clubs = new SelectList(clubs, "ClubId", "ClubName");

            return View();
        }



        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCLubFine(CreateClubFineViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            if (ModelState.IsValid)
            {
                var newClubFine = new Fine
                {
                    RuleViolated = viewModel.RuleViolated,
                    FineDetails = viewModel.FineDetails,
                    FineAmount = viewModel.FineAmount,
                    FineDuDate = viewModel.FineDuDate,
                    ClubId = viewModel.ClubId,
                    CreatedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = userId,
                    ModifiedDateTime = DateTime.Now,
                    PaymentStatus = PaymentStatus.Pending,
                    DivisionId = divisionId
                };

                _context.Add(newClubFine);
                await _context.SaveChangesAsync();

                var newSavedFine = await _context.Fines
                    .Where(f => f.ClubId == newClubFine.ClubId)
                    .Include(f => f.Club)
                    .FirstOrDefaultAsync();

                if (newSavedFine?.Club != null)
                {
                    var clubEmail = newSavedFine.Club.Email; 
                    var clubName = newSavedFine.Club.ClubName;
                    var fineAmount = newSavedFine.FineAmount;
                    var dueDate = newSavedFine.FineDuDate.ToShortDateString();

                    string emailBody = $@"
                Dear {clubName} club,<br/><br/>
                You have been fined for the following violation:<br/><br/>
                Rule Violated: {newSavedFine.RuleViolated}<br/>
                Fine Details: {newSavedFine.FineDetails}<br/>
                Fine Amount: {fineAmount}<br/>
                Due Date: {dueDate}<br/><br/>
                Please ensure that you pay the fine by the due date. If you have any questions, contact our support team.<br/><br/>
                Kind regards,<br/>
                Diski360 Finance Team
                   ";

                    BackgroundJob.Enqueue<EmailService>(service =>
                        service.SendEmailAsync(clubEmail, "Fine Notification", emailBody, "Diski 360"));
                }

                TempData["Message"] = $"You have successfully filed a fine/offence against {newSavedFine.Club.ClubName}. The club is required to pay an amount of {newSavedFine.FineAmount} before {newSavedFine.FineDuDate}. The email concerning this charge has been sent to the relevant club.";
                await _activityLogger.Log($"Charged {newSavedFine.Club.ClubName} R{newSavedFine.FineAmount} for {newSavedFine.RuleViolated}", user.Id);
                await _requestLogService.LogSuceededRequest("Successfully filed a club fine", StatusCodes.Status200OK);

                return RedirectToAction(nameof(ClubFines));
            }

            var clubs = _context.Club
                           .Where(c => c.DivisionId == divisionId)
                           .Select(c => new
                           {
                               ClubId = c.ClubId,
                               ClubName = c.ClubName
                           });

            ViewBag.Clubs = new SelectList(clubs, "ClubId", "ClubName");

            await _requestLogService.LogFailedRequest("Failed to file a club fine", StatusCodes.Status500InternalServerError);

            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdateClubFine (string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            var clubFine = await _context.Fines
                .Where(e => e.ClubId != null && e.OffenderId == null && e.FineId == decryptedFineId)
                .Include(e => e.Club)
                .FirstOrDefaultAsync();

            var club = clubFine.Club;


            var viewModel = new UpdateClubFineViewModel
            {
                FineId = decryptedFineId,
                ClubName = club.ClubName,
                RuleViolated = clubFine.RuleViolated,
                FineDetails = clubFine.FineDetails,
                FineAmount = clubFine.FineAmount,
                FineDueDate = clubFine.FineDuDate
            };

            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClubFine(UpdateClubFineViewModel viewModel)
        {
            if (viewModel.FineId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var existingClubFine = await _context.Fines
                .Where(e => e.ClubId != null && e.OffenderId == null && e.FineId == viewModel.FineId)
                .Include(e => e.Club)
                .FirstOrDefaultAsync();

            if (existingClubFine == null)
            {
                return NotFound();
            }

            if (ValidateClubUpdatedProperties(viewModel))
            {
                existingClubFine.RuleViolated = viewModel.RuleViolated;
                existingClubFine.FineDetails = viewModel.FineDetails;
                existingClubFine.FineAmount = viewModel.FineAmount;
                existingClubFine.FineDuDate = viewModel.FineDueDate;
                existingClubFine.ModifiedById = user.Id;
                existingClubFine.ModifiedDateTime = DateTime.Now;

                _context.Update(existingClubFine);
                await _context.SaveChangesAsync();

                var clubEmail = existingClubFine.Club.Email;
                var clubName = existingClubFine.Club.ClubName;
                var fineAmount = existingClubFine.FineAmount;
                var dueDate = existingClubFine.FineDuDate.ToShortDateString();

                string emailBody = $@"
            Dear {clubName} club,<br/><br/>
            Your fine details have been updated:<br/><br/>
            Rule Violated: {existingClubFine.RuleViolated}<br/>
            Fine Details: {existingClubFine.FineDetails}<br/>
            Fine Amount: {fineAmount}<br/>
            Due Date: {dueDate}<br/><br/>
            Please review the updated fine details. If you have any questions, contact our support team.<br/><br/>
            Kind regards,<br/>
            Diski360 Finance Team
                ";

                await _emailService.SendEmailAsync(
                    clubEmail,
                    "Club Fine Updated Notification",
                    emailBody, "Diski 360");
            }

            await _requestLogService.LogSuceededRequest("Successfully updated fine", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully updated {existingClubFine.Club.ClubName} charges";

            await _activityLogger.Log($"Updated {existingClubFine.Club.ClubName} fines", user.Id);
            return RedirectToAction(nameof(ClubFines));
        }


        private bool ValidateClubUpdatedProperties(UpdateClubFineViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.RuleViolated, new ValidationContext(viewModel, null, null) { MemberName = "RuleViolated" }, validationResults);
            Validator.TryValidateProperty(viewModel.FineDetails, new ValidationContext(viewModel, null, null) { MemberName = "FineDetails" }, validationResults);
            Validator.TryValidateProperty(viewModel.FineAmount, new ValidationContext(viewModel, null, null) { MemberName = "FineAmount" }, validationResults);
            Validator.TryValidateProperty(viewModel.FineDueDate, new ValidationContext(viewModel, null, null) { MemberName = "FineDueDate" }, validationResults);
            return validationResults.Count == 0;
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> MarkOverDueClubFine(string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            if (fineId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var existingClubFine = await _context.Fines
                .Where(e => e.ClubId != null && e.OffenderId == null && e.FineId == decryptedFineId)
                .Include(e => e.Club)
                .FirstOrDefaultAsync();

            if (existingClubFine == null)
            {
                return NotFound();
            }

            existingClubFine.PaymentStatus = PaymentStatus.Overdue;
            existingClubFine.ModifiedById = user.Id;
            existingClubFine.ModifiedDateTime = DateTime.Now;

            _context.Update(existingClubFine);
            await _context.SaveChangesAsync();

            var clubEmail = existingClubFine.Club.Email; 
            var clubName = existingClubFine.Club.ClubName;
            var fineAmount = existingClubFine.FineAmount;
            var dueDate = existingClubFine.FineDuDate.ToShortDateString();

            string emailBody = $@"
        Dear {clubName} club,<br/><br/>
        This is to notify you that your fine has been marked as overdue:<br/><br/>
        Rule Violated: {existingClubFine.RuleViolated}<br/>
        Fine Details: {existingClubFine.FineDetails}<br/>
        Fine Amount: {fineAmount}<br/>
        Due Date: {dueDate}<br/><br/>
        Please make the payment as soon as possible to avoid further actions.<br/><br/>
        Kind regards,<br/>
        Diski360 Finance Team
            ";

            await _emailService.SendEmailAsync(
                clubEmail,
                "Overdue Fine Notification",
                emailBody, "Diski 360");

            await _requestLogService.LogSuceededRequest("Successfully marked club fine as overdue", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully marked {existingClubFine.Club.ClubName} charges as overdue payment";

            await _activityLogger.Log($"Marked {existingClubFine.Club.ClubName} fines as overdue", user.Id);
            return RedirectToAction(nameof(ClubFines));
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> DropClubFine(string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            if (fineId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var existingClubFine = await _context.Fines
                .Where(e => e.ClubId != null && e.OffenderId == null && e.FineId == decryptedFineId)
                .Include(e => e.Club)
                .FirstOrDefaultAsync();

            if (existingClubFine == null)
            {
                return NotFound();
            }

            var clubEmail = existingClubFine.Club.Email; 
            var clubName = existingClubFine.Club.ClubName;
            var ruleViolated = existingClubFine.RuleViolated;

            string emailBody = $@"
        Dear {clubName} club,<br/><br/>
        We wanted to inform you that after full consideration, we have decided to drop your charges:<br/><br/>
        Rule Violated: {ruleViolated}<br/><br/>
        If you have any questions, please contact our support team.<br/><br/>
        Kind regards,<br/>
        Diski360 Support Team
            ";

            await _emailService.SendEmailAsync(
                clubEmail,
                "Fine Dropped Notification",
                emailBody, "Diski 360");

            await _requestLogService.LogSuceededRequest("Successfully dropped club fine", StatusCodes.Status200OK);


            TempData["Message"] = $"You have deleted {existingClubFine.Club.ClubName} fines of {existingClubFine.RuleViolated}";

            await _activityLogger.Log($"Dropped {existingClubFine.Club.ClubName} fines of {existingClubFine.RuleViolated}", user.Id);

            _context.Remove(existingClubFine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ClubFines));
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdateIndividualFine(string fineId)
        {

            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            var individualFine = await _context.Fines
                .Where(e => e.ClubId == null && e.OffenderId != null && e.FineId == decryptedFineId)
                .Include(e => e.Offender)
                .FirstOrDefaultAsync();

            var offender = individualFine.Offender;


            var viewModel = new UpdateIndividualFineViewModel
            {
                FineId = decryptedFineId,
                FullNames = $"{offender.FirstName} {offender.LastName}",
                RuleViolated = individualFine.RuleViolated,
                FineDetails = individualFine.FineDetails,
                FineAmount = individualFine.FineAmount,
                FineDueDate = individualFine.FineDuDate
            };

            return View(viewModel);
        }


        [Authorize(Roles = ("Personnel Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateIndividualFine(UpdateIndividualFineViewModel viewModel)
        {


            if (viewModel.FineId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var individualFine = await _context.Fines
                .Where(e => e.ClubId == null && e.OffenderId != null && e.FineId == viewModel.FineId)
                .Include(e => e.Offender)
                .FirstOrDefaultAsync();

            if (individualFine == null)
            {
                return NotFound();
            }

            if (ValidateIndividualUpdatedProperties(viewModel))
            {
                individualFine.RuleViolated = viewModel.RuleViolated;
                individualFine.FineDetails = viewModel.FineDetails;
                individualFine.FineAmount = viewModel.FineAmount;
                individualFine.FineDuDate = viewModel.FineDueDate;
                individualFine.ModifiedById = user.Id;
                individualFine.ModifiedDateTime = DateTime.Now;

                _context.Update(individualFine);
                await _context.SaveChangesAsync();

                var offenderEmail = individualFine.Offender.Email; 
                var offenderName = $"{individualFine.Offender.FirstName} {individualFine.Offender.LastName}";

                string emailBody = $@"
            Hi {offenderName},<br/><br/>
            We wanted to inform you that your fine has been updated:<br/><br/>
            Rule Violated: {individualFine.RuleViolated}<br/>
            Fine Amount: {individualFine.FineAmount}<br/>
            Due Date: {individualFine.FineDuDate.ToShortDateString()}<br/><br/>
            If you have any questions, please contact our support team.<br/><br/>
            Kind regards,<br/>
            Diski360 Finance Team
                ";

                await _emailService.SendEmailAsync(
                    offenderEmail,
                    "Fine Updated Notification",
                    emailBody, "Diski 360");
            }

            await _requestLogService.LogSuceededRequest("Successfully updated individual fine", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully updated {viewModel.FullNames} charges";

            await _activityLogger.Log($"Updated {viewModel.FullNames} fines", user.Id);

            return RedirectToAction(nameof(IndividualFines));
        }


        private bool ValidateIndividualUpdatedProperties(UpdateIndividualFineViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.RuleViolated, new ValidationContext(viewModel, null, null) { MemberName = "RuleViolated" }, validationResults);
            Validator.TryValidateProperty(viewModel.FineDetails, new ValidationContext(viewModel, null, null) { MemberName = "FineDetails" }, validationResults);
            Validator.TryValidateProperty(viewModel.FineAmount, new ValidationContext(viewModel, null, null) { MemberName = "FineAmount" }, validationResults);
            Validator.TryValidateProperty(viewModel.FineDueDate, new ValidationContext(viewModel, null, null) { MemberName = "FineDueDate" }, validationResults);
            return validationResults.Count == 0;
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> MarkOverDueIndividualFine(string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            if (fineId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var individualFine = await _context.Fines
                .Where(e => e.ClubId == null && e.OffenderId != null && e.FineId == decryptedFineId)
                .Include(e => e.Offender)
                .FirstOrDefaultAsync();

            if (individualFine == null)
            {
                return NotFound();
            }

            individualFine.PaymentStatus = PaymentStatus.Overdue;
            individualFine.ModifiedById = user.Id;
            individualFine.ModifiedDateTime = DateTime.Now;

            _context.Update(individualFine);
            await _context.SaveChangesAsync();

            var offenderEmail = individualFine.Offender.Email; 
            var offenderName = $"{individualFine.Offender.FirstName} {individualFine.Offender.LastName}";

            string emailBody = $@"
        Hi {offenderName},<br/><br/>
        We wanted to inform you that your fine has been marked as overdue:<br/><br/>
        Rule Violated: {individualFine.RuleViolated}<br/>
        Fine Amount: {individualFine.FineAmount}<br/>
        Due Date: {individualFine.FineDuDate.ToShortDateString()}<br/><br/>
        Please make the payment as soon as possible to avoid further actions.<br/><br/>
        If you have any questions, please contact our support team.<br/><br/>
        Kind regards,<br/>
        Diski360 Finance Team
            ";

            await _emailService.SendEmailAsync(
                offenderEmail,
                "Fine Overdue Notification",
                emailBody, "Diski 360");

            await _requestLogService.LogSuceededRequest("Successfully marked individual fine as overdue", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully marked {individualFine.Offender.FirstName} {individualFine.Offender.LastName}'s charges as overdue.";

            await _activityLogger.Log($"Marked {individualFine.Offender.FirstName} {individualFine.Offender.LastName} fines as overdue", user.Id);

            return RedirectToAction(nameof(IndividualFines));
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> DropIndividualFine(string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);

            if (fineId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var individualFine = await _context.Fines
                .Where(e => e.ClubId == null && e.OffenderId != null && e.FineId == decryptedFineId)
                .Include(e => e.Offender)
                .FirstOrDefaultAsync();

            if (individualFine == null)
            {
                return NotFound();
            }

            var offenderEmail = individualFine.Offender.Email;
            var offenderName = $"{individualFine.Offender.FirstName} {individualFine.Offender.LastName}";

            string emailBody = $@"
        Hi {offenderName},<br/><br/>
        We wanted to inform you that the fine against you has been dropped:<br/><br/>
        Rule Violated: {individualFine.RuleViolated}<br/>
        Fine Amount: {individualFine.FineAmount}<br/><br/>
        If you have any questions, please contact our support team.<br/><br/>
        Kind regards,<br/>
        Diski360 Finance Team
            ";

            await _emailService.SendEmailAsync(
                offenderEmail,
                "Fine Dropped Notification",
                emailBody, "Diski 360");

            await _requestLogService.LogSuceededRequest("Successfully dropped individual fine", StatusCodes.Status200OK);

            TempData["Message"] = $"You have deleted {individualFine.Offender.FirstName} {individualFine.Offender.LastName}'s fine for {individualFine.RuleViolated}.";

            await _activityLogger.Log($"Dropped {individualFine.Offender.FirstName} {individualFine.Offender.LastName}'s fine for {individualFine.RuleViolated}", user.Id);

            _context.Remove(individualFine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(IndividualFines));
        }


        private bool FineExists(int id)
        {
            return (_context.Fines?.Any(e => e.FineId == id)).GetValueOrDefault();
        }
    }
}
