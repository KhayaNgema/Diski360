using Azure.Core;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;
using System.Text.Encodings.Web;
using System.Text;
using Polly;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authorization;

namespace MyField.Controllers
{
    public class DivisionsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly IEncryptionService _encryptionService;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUserStore<UserBaseModel> _userStore;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RandomPasswordGeneratorService _passwordGenerator;
        private readonly RequestLogService _requestLogService;

        public DivisionsController(
      Ksans_SportsDbContext context,
      FileUploadService fileUploadService,
      UserManager<UserBaseModel> userManager,
      IActivityLogger activityLogger,
      IEncryptionService encryptionService,
      EmailService emailService,
      IWebHostEnvironment hostingEnvironment,
      IUserStore<UserBaseModel> userStore,
      RoleManager<IdentityRole> roleManager,
      RandomPasswordGeneratorService passwordGenerator,
      RequestLogService requestLogService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _encryptionService = encryptionService;
            _emailService = emailService;
            _hostingEnvironment = hostingEnvironment;
            _userStore = userStore;
            _roleManager = roleManager;
            _passwordGenerator = passwordGenerator;
            _requestLogService = requestLogService;
        }

        [Authorize(Roles ="Division Manager")]
        [HttpGet]
        public async Task<IActionResult> ManageDivision()
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

            return View(division);
        }


        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> ManageDivisions()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisions = await _context.Divisions
                .Include(d => d.CreatedBy)
                .Include(d => d.ModifiedBy)
                .ToListAsync();

            return View(divisions);
        }



        public async Task<IActionResult> SoccerDivisions()
        {
            return PartialView("SoccerDivisionsPartial");
        }

        public async Task<IActionResult> NetballDivisions()
        {
            return PartialView("NetballDivisionsPartial");
        }

        public async Task<IActionResult> FindSoccerDivisions()
        {
            var soccerDivisions = await _context.Divisions
                .Where(sd => sd.DivisionType == DivisionType.Soccer)
                .ToListAsync();

            return PartialView("_SoccerDivisionsPartial", soccerDivisions);
        }

        public async Task<IActionResult> FindNetballDivisions()
        {
            var netballDivisions = await _context.Divisions
              .Where(sd => sd.DivisionType == DivisionType.Netball)
              .ToListAsync();

            return PartialView("_NetballDivisionsPartial", netballDivisions);
        }


        public async Task<IActionResult> Divisions()
        {
            var divisions = await _context.Divisions
                .ToListAsync();

            return View(divisions);
        }

        public async Task<IActionResult> DivisionDetails(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            var divisionManager = await _context.DivisionManagers
                .Where(dm => dm.Division.DivisionId == decryptedDivisionId)
                .Include(da => da.Division)
                .FirstOrDefaultAsync();

            var divisionAggreement = await _context.DivisionAggreements
                .Where(da => da.DivisionId == decryptedDivisionId)
                .Include(da => da.Division)
                .FirstOrDefaultAsync();

            var viewModel = new DivisionDetailsViewModel
            {
                DivisionName = division?.DivisionName,
                DivisionAbbr = division?.DivisionAbbr,
                DivisionBadge = division?.DivisionBadge,
                DivisionType = division.DivisionType,
                Address = division?.Address,
                ManagerFirstName = divisionManager?.FirstName,
                ManagerLastName = divisionManager?.LastName,
                ManagerEmail = divisionManager?.Email,
                ManagerPhone = divisionManager?.PhoneNumber,
                Contract = divisionAggreement?.SignedContract,
                DivisionDescription = division.DivisionDescription
            };

            return View(viewModel);
        }


        public IActionResult RequestSubmittedSuccessfully(string referenceNumber)
        {
            ViewBag.ReferenceNumber = referenceNumber;
            return View();
        }


        public async Task<IActionResult> OnboardingRequests()
        {
            var onboardingRequests = await _context.OnboardingRequests.ToListAsync();

            var onboardingRequestsViewModel = onboardingRequests.Select(req => new OnboardingRequestsViewModel
            {
                RequestId = req.RequestId,
                ReferenceNumber = req.ReferenceNumber,
                DivisionName = req.DivisionName,
                DivisionAbbr = req.DivisionAbbr,
                Status = req.RequestStatus,
                DateTime = req.CreatedDateTime,
                ManagerName = $"{req.ManagerFirstName} {req.ManagerLastName}"
            }).ToList();

            var onboardingRequestsReport = await _context.OnboardingRequestsReports.FirstOrDefaultAsync();

            var onboardingRequestCount = await _context.OnboardingRequests.CountAsync();

            var pendingOnboardingRequestsCount = await _context.OnboardingRequests
                .Where(ar => ar.RequestStatus == RequestStatus.Pending)
                .CountAsync();

            var approvedOnboardingRequestsCount = await _context.OnboardingRequests
                .Where(ar => ar.RequestStatus == RequestStatus.Approved)
                .CountAsync();

            var rejectedOnboardingRequestsCount = await _context.OnboardingRequests
                .Where(rr => rr.RequestStatus == RequestStatus.Rejected)
                .CountAsync();

            var completedOnboardingRequestsCount = await _context.OnboardingRequests
                .Where(rr => rr.RequestStatus == RequestStatus.Completed)
                .CountAsync();

            var onboardingRequestsReportsViewModel = new OnboardingRequestsReportViewModel
            {
                TotalOnboardingRequests = onboardingRequestCount,
                ApprovedOnboardingRequests = approvedOnboardingRequestsCount,
                RejectedOnboardingRequests = rejectedOnboardingRequestsCount,
                CompletedOnboardingRequests = completedOnboardingRequestsCount,
                PendingOnboardingRequests = pendingOnboardingRequestsCount
            };

            var combinedViewModel = new CombinedOnboardingRequestViewModel
            {
                OnboardingRequestsViewModel = onboardingRequestsViewModel,
                OnboardingRequestsReportViewModel = onboardingRequestsReportsViewModel
            };

            var combinedViewModels = new List<CombinedOnboardingRequestViewModel> { combinedViewModel };

            return View(combinedViewModels);
        }



        [HttpGet]
        public async Task<IActionResult> MakeOnboardingRequest()
        {
            ViewData["DivisionTypes"] = Enum.GetValues(typeof(DivisionType))
                    .Cast<DivisionType>()
                    .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeOnboardingRequest(OnboardingRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newOnboardingRequest = new OnboardingRequest
                {
                    DivisionName = viewModel.DivisionName,
                    DivisionDescription = viewModel.DivisionDescription,
                    Address = $"{viewModel.AddressLine_1}, {viewModel.AddressLine_2}, {viewModel.Suburb}, {viewModel.Town_City}, {viewModel.Province}, {viewModel.ZipCode}",
                    DivisionType = viewModel.DivisionType,
                    ManagerFirstName = viewModel.ManagerFirstName,
                    ManagerLastName = viewModel.ManagerLastName,
                    ManagerEmail = viewModel.ManagerEmail,
                    ManagerPhoneNumber = viewModel.ManagerPhoneNumber,
                    DateOfBirth = viewModel.DateOfBirth,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    RequestStatus = RequestStatus.Pending,
                    DivisionBadge = "Images/division_logo.jpeg",
                    DivisionAbbr = viewModel.DivisionAbbr
                };

                newOnboardingRequest.ReferenceNumber = GenerateOnboardingrequestReferenceNumber();

                if (viewModel.DivisionBadges != null && viewModel.DivisionBadges.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(viewModel.DivisionBadges);
                    newOnboardingRequest.DivisionBadge = uploadedImagePath;
                }

                _context.Add(newOnboardingRequest);
                await _context.SaveChangesAsync();

                var onboardingRequest = await _context.OnboardingRequests
                    .Where(or => or.Equals(newOnboardingRequest))
                    .FirstOrDefaultAsync();
                var referenceNumber = onboardingRequest?.ReferenceNumber;

                var subject = "Your Onboarding Request Has Been Submitted to Diski360";
                var emailBodyTemplate = $@"
        Dear {viewModel.ManagerFirstName} {viewModel.ManagerLastName},<br/><br/>
        We are pleased to inform you that your onboarding request for the division <strong>{viewModel.DivisionName}</strong> has been successfully submitted to Diski360.<br/><br/>
        Your request is now under review, and the following reference number has been generated for your request: <strong>{referenceNumber}</strong>.<br/>
        Please keep this reference number safe, as you can use it to track the progress of your onboarding request.<br/><br/>
        What happens next?<br/>
        If your request is approved, you will receive another email from us containing a formal contract that needs to be signed and submitted at your earliest convenience.<br/>
        This is a critical step in finalizing your division's onboarding with Diski360, so we encourage you to monitor your inbox for further communications.<br/><br/>
        Thank you for choosing Diski360. We look forward to successfully onboarding your division.<br/><br/>
        Best regards,<br/>
        The Diski360 Team
        ";

                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(
                    viewModel.ManagerEmail,
                    subject,
                    emailBodyTemplate, "Diski 360"));

                await _requestLogService.LogSuceededRequest("Successfully submitted the onboarding request", StatusCodes.Status200OK); 

                return RedirectToAction(nameof(RequestSubmittedSuccessfully), new { referenceNumber });
            }

            ViewData["DivisionTypes"] = Enum.GetValues(typeof(DivisionType))
                .Cast<DivisionType>()
                .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            await _requestLogService.LogFailedRequest("Failed to submit onboarding request", StatusCodes.Status500InternalServerError);

            return View(viewModel);
        }



        private string GenerateOnboardingrequestReferenceNumber()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var randomLetters = new string(Enumerable.Repeat(letters, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{year}{month}{day}{randomLetters}";
        }

        [HttpGet]
        public async Task<IActionResult> ReviewRequest(string requestId)
        {
            if(requestId == null)
            {
                return NotFound();
            }

            var decryptedRequestId = _encryptionService.DecryptToInt(requestId);

            var onboardingRequest = await _context.OnboardingRequests.FindAsync(decryptedRequestId);

            var viewModel = new ReviewOnboardingRequestViewModel
            {
                RequestId = decryptedRequestId,
                DivisionBadge = onboardingRequest.DivisionBadge,
                DivisionName = onboardingRequest.DivisionName,
                DivisionDescription = onboardingRequest.DivisionDescription,
                DivisionType = onboardingRequest.DivisionType,
                ManagerFirstName = onboardingRequest.ManagerFirstName,
                ManagerLastName = onboardingRequest.ManagerLastName,
                ManagerEmail = onboardingRequest.ManagerEmail,
                ManagerPhoneNumber = onboardingRequest.ManagerPhoneNumber,
                RequestDate = onboardingRequest.CreatedDateTime,
                RequestStatus = onboardingRequest.RequestStatus,
                Address = onboardingRequest.Address,
                DivisionAbbr = onboardingRequest.DivisionAbbr,
                DateOfBirth = onboardingRequest.DateOfBirth,
                RefenceNumber = onboardingRequest.ReferenceNumber
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(string requestId)
        {
            if(ModelState.IsValid)
            {
                if (requestId == null)
                {
                    return NotFound();
                }

                var decryptedRequestId = _encryptionService.DecryptToInt(requestId);

                var onboardingRequest = await _context.OnboardingRequests.FindAsync(decryptedRequestId);

                if (onboardingRequest == null)
                {
                    return NotFound();
                }

                onboardingRequest.RequestStatus = RequestStatus.Approved;

                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);

                await _activityLogger.Log($"Approved an onboarding request to onboard {onboardingRequest.DivisionName} {onboardingRequest.DivisionType} division", user.Id);

                var managerSubject = "Your Onboarding Request has been Approved";
                var managerEmailBody = $@"
Dear {onboardingRequest.ManagerFirstName} {onboardingRequest.ManagerLastName},<br/><br/>
We are pleased to inform you that your onboarding request for the division <strong>{onboardingRequest.DivisionName}</strong> has been approved by Diski360.<br/><br/>
Attached to this email, you will find the formal contract for you to review, sign, and return at your earliest convenience.<br/><br/>
Thank you for your patience and cooperation.<br/><br/>
Best regards,<br/>
The Diski360 Team
";

                var userSubject = "Request Approved: Onboarding Division";
                var userEmailBody = $@"
Hi {user.FirstName} {user.LastName},<br/><br/>
You have successfully approved the onboarding request for the division <strong>{onboardingRequest.DivisionName}</strong>.<br/><br/>
Division Type: {onboardingRequest.DivisionType}<br/><br/>
The division has been added to Diski360, and the next step involves sending a formal contract to the division manager. You will receive further updates as the process progresses.<br/><br/>
Thank you for your prompt action.<br/><br/>
Best regards,<br/>
The Diski360 Team
";

                var contractFileNames = new List<string>
    {
        "AGREEMENT BETWEEN DISKI360 AND LEAGUE ASSOCIATION NAME_FINAL AGREEMENT.docx",

    };

                var rootPath = _hostingEnvironment.WebRootPath;
                var attachmentPaths = contractFileNames
                    .Select(fileName => Path.Combine(rootPath, "UploadedFiles", fileName))
                    .ToList();

                BackgroundJob.Enqueue(() => _emailService.SendEmailWithAttachmentsAsync(
                    onboardingRequest.ManagerEmail,
                    managerSubject,
                    managerEmailBody,
                    attachmentPaths, "Diski 360"));

                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(
                    user.Email,
                    userSubject,
                    userEmailBody, "Diski 360"));

                await _requestLogService.LogSuceededRequest("Successfully approved onboarding request", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully approved the request to onboard {onboardingRequest.DivisionName} {onboardingRequest.DivisionType} division into Diski360.";

                return RedirectToAction(nameof(OnboardingRequests));
            }

            await _requestLogService.LogFailedRequest("Failed to approve onboarding request", StatusCodes.Status500InternalServerError);

            TempData["Message"] = $"Failed to approve onboarding request";

            return RedirectToAction(nameof(OnboardingRequests));
        }



        [HttpPost]
        public async Task<IActionResult> RejectRequest(string requestId)
        {
            if(ModelState.IsValid)
            {
                if (requestId == null)
                {
                    return NotFound();
                }

                var decryptedRequestId = _encryptionService.DecryptToInt(requestId);

                var user = await _userManager.GetUserAsync(User);
                var onboardingRequest = await _context.OnboardingRequests.FindAsync(decryptedRequestId);

                if (onboardingRequest == null)
                {
                    return NotFound();
                }

                onboardingRequest.RequestStatus = RequestStatus.Rejected;
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Rejected an onboarding request to onboard {onboardingRequest.DivisionName} {onboardingRequest.DivisionType} division.", user.Id);

                var subject = "Onboarding Request Rejected";
                var userEmailBody = $@"
        Hi {user.FirstName},<br/><br/>
        We regret to inform you that the onboarding request for the division '{onboardingRequest.DivisionName}' cannot be processed further. Unfortunately, the request has been rejected.<br/><br/>
        If you have any questions or need further clarification, please feel free to contact us.<br/><br/>
        Regards,<br/>
        Diski360 Team
    ";

                var managerEmailBody = $@"
        Hi,<br/><br/>
        We regret to inform you that the onboarding request for the division '{onboardingRequest.DivisionName}' has been rejected and cannot be processed further.<br/><br/>
        If you have any questions or need further information, please feel free to contact us.<br/><br/>
        Regards,<br/>
        Diski360 Team
    ";

                var divisionManagerEmail = onboardingRequest.ManagerEmail;
                var emailService = new EmailService();

                BackgroundJob.Enqueue(() => emailService.SendEmailAsync(user.Email, subject, userEmailBody, "Diski 360"));

                if (!string.IsNullOrEmpty(divisionManagerEmail))
                {
                    BackgroundJob.Enqueue(() => emailService.SendEmailAsync(divisionManagerEmail, subject, managerEmailBody, "Diski 360"));
                }

                await _requestLogService.LogSuceededRequest("Successfully rejected onboarding request", StatusCodes.Status200OK);

                TempData["Message"] = $"You have rejected a request to onboard the division '{onboardingRequest.DivisionName}' into Diski360.";

                return RedirectToAction(nameof(OnboardingRequests));
            }

            await _requestLogService.LogFailedRequest("Failed to reject onboarding request", StatusCodes.Status500InternalServerError);

            TempData["Message"] = $"Failed to reject onboarding request";

            return RedirectToAction(nameof(OnboardingRequests));
        }

        [HttpGet]
        public async Task<IActionResult> OnboardDivisionAndManager(string requestId)
        {
            if(requestId == null) { return NotFound(); }

            var decryptedRequestId = _encryptionService.DecryptToInt(requestId) ;

            var onboardingRequest = await _context.OnboardingRequests
                .Where(or => or.RequestId == decryptedRequestId)
                .FirstOrDefaultAsync();

            var viewModel = new OnboardDivisionViewModel
            {
                RequestId = decryptedRequestId,
                DivisionName =  onboardingRequest.DivisionName,
                DivisionDescription = onboardingRequest.DivisionDescription,
                DivisionBadge = onboardingRequest.DivisionBadge,
                DivisionType = onboardingRequest.DivisionType,
                ManagerFirstName = onboardingRequest.ManagerFirstName,
                ManagerLastName = onboardingRequest.ManagerLastName,
                DateOfBirth = onboardingRequest.DateOfBirth,
                ManagerEmail = onboardingRequest.ManagerEmail,
                ManagerPhoneNumber = onboardingRequest.ManagerPhoneNumber,
                RefenceNumber = onboardingRequest.ReferenceNumber,
                RequestDate = onboardingRequest.CreatedDateTime,
                Address = onboardingRequest.Address,
                RequestStatus = onboardingRequest.RequestStatus,
                DivisionAbbr = onboardingRequest.DivisionAbbr
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnboardDivisionAndManager(OnboardDivisionViewModel viewModel, string returnUrl = null)
        {
            try
            {
                if (viewModel.RequestId == null)
                {
                    return NotFound();
                }

                var loggedInUser = await _userManager.GetUserAsync(User);

                var onboardingRequest = await _context.OnboardingRequests
                    .Where(or => or.RequestId == viewModel.RequestId)
                    .FirstOrDefaultAsync();

                if (onboardingRequest == null)
                {
                    return NotFound();
                }

                var newDivision = new Division
                {
                    DivisionName = onboardingRequest.DivisionName,
                    DivisionDescription = onboardingRequest.DivisionDescription,
                    DivisionBadge = onboardingRequest.DivisionBadge,
                    DivisionAbbr = onboardingRequest.DivisionAbbr,
                    Address = onboardingRequest.Address,
                    DivisionType = onboardingRequest.DivisionType,
                    CreatedDateTime = onboardingRequest.CreatedDateTime,
                    CreatedById = loggedInUser.Id,
                    IsActive = false,
                    IsSuspended = false,
                    HasPaid = false,
                    ModifiedById = loggedInUser.Id,
                    ModifiedDateTime = DateTime.Now,
                    Status = DivisionStatus.Active,
                };

                await _activityLogger.Log($"Added {onboardingRequest.DivisionName} division into Diski360.", loggedInUser.Id);

                _context.Add(newDivision);
                await _context.SaveChangesAsync();

                var savedDivision = await _context.Divisions
                    .FirstOrDefaultAsync(sd => sd.Equals(newDivision));

                if (savedDivision == null)
                {
                    return NotFound();
                }



                var generatedPassword = _passwordGenerator.GenerateRandomPassword();

                var newDivisionManager = new DivisionManager
                {
                    UserName = onboardingRequest.ManagerEmail,
                    Email = onboardingRequest.ManagerEmail,
                    FirstName = onboardingRequest.ManagerFirstName,
                    LastName = onboardingRequest.ManagerLastName,
                    DateOfBirth = onboardingRequest.DateOfBirth,
                    PhoneNumber = onboardingRequest.ManagerPhoneNumber,
                    EmailConfirmed = false,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    IsDeleted = false,
                    IsSuspended = false,
                    IsActive = true,
                    IsFirstTimeLogin = true,
                    DivisionId = savedDivision.DivisionId
                };

                var overallNewsReport = new OverallNewsReport
                {
                    AuthoredNewsCount = 0,
                    ApprovedNewsCount = 0,
                    PublishedNewsCount = 0,
                    RejectedNewsCount = 0,
                    NewsReadersCount = 0,
                    ApprovedNewsRate = 0,
                    PublishedNewsRate = 0,
                    RejectedNewsRate = 0,
                    DivisionId = savedDivision.DivisionId
                };

                if (!await _context.OverallNewsReports.AnyAsync())
                {
                    _context.OverallNewsReports.Add(overallNewsReport);
                    await _context.SaveChangesAsync();
                }

                var personnelAccountsReport = new PersonnelAccountsReport
                {
                    OverallAccountsCount = 0,
                    ActiveAccountsCount = 0,
                    InactiveAccountsCount = 0,
                    SuspendedAccountsCount = 0,
                    ActiveAccountsRate = 0,
                    InactiveAccountsRate = 0,
                    SuspendedAccountsRate = 0,
                    DivisionId = savedDivision.DivisionId
                };

                if (!await _context.PersonnelAccountsReports.AnyAsync())
                {
                    _context.PersonnelAccountsReports.Add(personnelAccountsReport);
                    await _context.SaveChangesAsync();
                }

                var personnelFinancialReport = new PersonnelFinancialReport
                {
                    ExpectedRepayableAmount = 0,
                    PaidPaymentFinesCount = 0,
                    PendingPaymentFinesCount = 0,
                    OverduePaymentFineCount = 0,
                    TotalPaidAmount = 0,
                    TotalUnpaidAmount = 0,
                    RepayableFinesCount = 0,
                    OverdueFinesRate = 0,
                    PaidFinesRate = 0,
                    PendingFinesRate = 0,
                    DivisionId = savedDivision.DivisionId
                };

                if (!await _context.PersonnelFinancialReports.AnyAsync())
                {
                    _context.PersonnelFinancialReports.Add(personnelFinancialReport);
                    await _context.SaveChangesAsync();
                }


                var fansAccountsReport = new FansAccountsReport
                {
                    OverallFansAccountsCount = 0,
                    ActiveFansAccountsCount = 0,
                    InactiveFansAccountsCount = 0,
                    SuspendedFansAccountsCount = 0,
                    ActiveFansAccountsRate = 0,
                    InactiveFansAccountsRate = 0,
                    SuspendedFansAccountsRate = 0,
                    DivisionId = savedDivision.DivisionId
                };

                onboardingRequest.RequestStatus = RequestStatus.Completed;

                _context.Update(onboardingRequest);
                await _context.SaveChangesAsync();

                var newAgreement = new DivisionAggreement
                {
                    DivisionId = savedDivision.DivisionId,
                    AgreementStartDate = viewModel.AgreementStartDate,
                    AgreementEndDate = viewModel.AgreementEndDate,
                    CreatedById = loggedInUser.Id,
                    ModifiedById = loggedInUser.Id,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now
                };

                if (viewModel.SignedContracts != null && viewModel.SignedContracts.Length > 0)
                {
                    var uploadedContractPath = await _fileUploadService.UploadFileAsync(viewModel.SignedContracts);
                    newAgreement.SignedContract = uploadedContractPath;


                    var newlySavedDivision = await _context.Divisions
                        .Where(ns => ns.Equals(savedDivision))
                        .FirstOrDefaultAsync();

                    newlySavedDivision.SignedContract = uploadedContractPath;

                    _context.Update(newlySavedDivision);
                }

                _context.Add(newAgreement);
                await _context.SaveChangesAsync();


                if (!await _context.FansAccountsReports.AnyAsync())
                {
                    _context.FansAccountsReports.Add(fansAccountsReport);
                    await _context.SaveChangesAsync();
                }

                var result = await _userManager.CreateAsync(newDivisionManager, generatedPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newDivisionManager, "Division Manager");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newDivisionManager);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = newDivisionManager.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string accountCreationEmailBody = $"Hello {onboardingRequest.ManagerFirstName} {onboardingRequest.ManagerLastName},<br><br>";
                    accountCreationEmailBody += $"Welcome to Diski360!<br><br>";
                    accountCreationEmailBody += $"You have been added as a Division Manager for the '{savedDivision.DivisionName}' division.<br><br>";
                    accountCreationEmailBody += $"Your login credentials are as follows:<br>";
                    accountCreationEmailBody += $"Email: {onboardingRequest.ManagerEmail}<br>";
                    accountCreationEmailBody += $"Password: {generatedPassword}<br><br>";
                    accountCreationEmailBody += "Please note that we have sent you two emails, including this one. You need to open the other email to confirm your email address before you can log into the system.<br><br>";
                    accountCreationEmailBody += "Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(newDivisionManager.Email, "Welcome to Diski360", accountCreationEmailBody, "Diski 360"));

                    string emailConfirmationEmailBody = $"Hello {newDivisionManager.FirstName},<br><br>";
                    emailConfirmationEmailBody += $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>";
                    emailConfirmationEmailBody += "Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(newDivisionManager.Email, "Confirm Your Email Address", emailConfirmationEmailBody, "Diski 360"));

                    await _activityLogger.Log($"Added {newDivisionManager.FirstName} {newDivisionManager.LastName} as Division Manager for the '{savedDivision.DivisionName}' division", loggedInUser.Id);

                    await _context.SaveChangesAsync();

                    await _requestLogService.LogSuceededRequest($"Successfully onboarded division and its manager.", StatusCodes.Status200OK);

                    TempData["Message"] = $"You have successfully onboarded'{onboardingRequest.DivisionName}' division and its manager into Diski360.";

                    return RedirectToAction(nameof(OnboardingRequests));
                }

                await _requestLogService.LogFailedRequest("Failed to onboard division and its manager.", StatusCodes.Status500InternalServerError);

                ModelState.AddModelError(string.Empty, "Failed to create the user.");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to onboard division and manager: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }

        public async Task<IActionResult> ActivateDivision(string divisionId)
        {


            if(divisionId == null)
            {
                return NotFound();
            }

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status = DivisionStatus.Active;
            division.IsActive = true;
            
            _context.Update(division);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Activated {division.DivisionName} division.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully activated a division.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully activated {division.DivisionName} division.";

            return RedirectToAction(nameof(ManageDivisions));
        }


        public async Task<IActionResult> DeActivateDivision(string divisionId)
        {
            if (divisionId == null)
            {
                return NotFound();
            }

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status = DivisionStatus.Inactive;
            division.IsActive = false;

            _context.Update(division);
            await _context.SaveChangesAsync();


            await _activityLogger.Log($"DeActivated {division.DivisionName} division.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully deactivated a division.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have deactivated {division.DivisionName} division.";

            return RedirectToAction(nameof(ManageDivisions));
        }

        public async Task<IActionResult> SuspendDivision(string divisionId)
        {
            if (divisionId == null)
            {
                return NotFound();
            }

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status = DivisionStatus.Suspended;

            _context.Update(division);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Suspended {division.DivisionName} division.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully suspended a division.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have suspended {division.DivisionName} division.";

            return RedirectToAction(nameof(ManageDivisions));
        }

        public async Task<IActionResult> MarkAsPaidDivision(string divisionId)
        {
            if (divisionId == null)
            {
                return NotFound();
            }

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status = DivisionStatus.Active;
            division.IsActive = true;
            division.HasPaid = true;

            _context.Update(division);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Marked {division.DivisionName} division as paid.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully marked a division as paid.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully marked {division.DivisionName} division as paid.";

            return RedirectToAction(nameof(ManageDivisions));
        }


        public async Task<IActionResult> MarkAsUnpaidDivision(string divisionId)
        {
            if (divisionId == null)
            {
                return NotFound();
            }

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status = DivisionStatus.Inactive;
            division.IsActive = false;
            division.HasPaid = false;

            _context.Update(division);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Marked {division.DivisionName} division as unpaid.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully marked a division as unpaid.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully marked {division.DivisionName} division as unpaid.";

            return RedirectToAction(nameof(ManageDivisions));
        }

        public async Task<IActionResult> UnsuspendDivision(string divisionId)
        {
            if (divisionId == null)
            {
                return NotFound();
            }
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status  = DivisionStatus.Active;

            _context.Update(division);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Unsuspended {division.DivisionName} division.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully unsuspended a division.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully unsuspended {division.DivisionName} division.";

            return RedirectToAction(nameof(ManageDivisions));
        }

        public async Task<IActionResult> DeleteDivision(string divisionId)
        {
            if (divisionId == null)
            {
                return NotFound();
            }
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var user = await _userManager.GetUserAsync(User);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            division.Status = DivisionStatus.Deleted;

            _context.Update(division);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Deleted {division.DivisionName} division.", user.Id);

            await _requestLogService.LogSuceededRequest($"Successfully deleted a division.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have deleted {division.DivisionName} division.";

            return RedirectToAction(nameof(ManageDivisions));
        }

        
        [Authorize(Roles ="Division Manager")]
        [HttpGet]
        public async Task<IActionResult> UpdateDivision(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var division = await _context.Divisions
                .Where(d => d.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            var viewModel = new UpdateDivisionViewModel
            {
                DivisionBadges = division.DivisionBadge,
                DivisionAbbr = division.DivisionAbbr,
                DivisionId = division.DivisionId,
                DivisionName = division.DivisionName,
                DivisionDescription = division.DivisionDescription
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Division Manager")]
        [HttpPost]
        public async Task<IActionResult> UpdateDivision(UpdateDivisionViewModel viewModel, IFormFile DivisionBadgeFile)
        {
            if (viewModel == null)
            {
                return NotFound();
            };

           if(ModelState.IsValid)
           {
                var user = await _userManager.GetUserAsync(User);

                var division = await _context.Divisions
                    .Where(d => d.DivisionId == viewModel.DivisionId)
                    .FirstOrDefaultAsync();

                division.DivisionName = viewModel.DivisionName;
                division.DivisionAbbr = viewModel.DivisionAbbr;
                division.DivisionDescription = viewModel.DivisionDescription;
                division.ModifiedDateTime = DateTime.Now;
                division.ModifiedById = user.Id;

                if (DivisionBadgeFile != null && DivisionBadgeFile.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(DivisionBadgeFile);
                    division.DivisionBadge = uploadedImagePath;
                }

                _context.Update(division);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Updated {division.DivisionName} division.", user.Id);

                await _requestLogService.LogSuceededRequest($"Successfully updated a division.", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully updated {division.DivisionName} division information.";

                return RedirectToAction(nameof(ManageDivision));
           };

           return View(viewModel);
        }
    }
}
