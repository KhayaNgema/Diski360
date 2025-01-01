using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;

namespace MyField.Areas.Identity.Pages.Account
{
    public class RegisterDivisionManagerModel : PageModel
    {
        private readonly SignInManager<UserBaseModel> _signInManager;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IUserStore<UserBaseModel> _userStore;
        private readonly IUserEmailStore<UserBaseModel> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly FileUploadService _fileUploadService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RandomPasswordGeneratorService _passwordGenerator;
        private readonly IEmailSender _emailSender;
        private readonly EmailService _emailService;
        private readonly Ksans_SportsDbContext _context;
        private readonly IActivityLogger _activityLogger;
        private readonly RequestLogService _requestLogService;

        public RegisterDivisionManagerModel(
            UserManager<UserBaseModel> userManager,
            IUserStore<UserBaseModel> userStore,
            SignInManager<UserBaseModel> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            FileUploadService fileUploadService,
            RoleManager<IdentityRole> roleManager,
            RandomPasswordGeneratorService passwordGenerator,
            EmailService emailService,
            Ksans_SportsDbContext db,
            IActivityLogger activityLogger,
            RequestLogService requestLogService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _fileUploadService = fileUploadService;
            _roleManager = roleManager;
            _passwordGenerator = passwordGenerator;
            _emailService = emailService;
            _context = db;
            _activityLogger = activityLogger;
            _requestLogService = requestLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Date of birth")]
            public DateTime DateOfBirth { get; set; }

            [Display(Name = "Profile picture")]
            public IFormFile? ProfilePicture { get; set; }

            [Required]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Club")]
            public int SelectedDivisionId { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var loggedInUser = await _userManager.GetUserAsync(User);

            var divisionId = (loggedInUser as ClubAdministrator)?.DivisionId ??
                             (loggedInUser as ClubManager)?.DivisionId ??
                             (loggedInUser as Player)?.DivisionId ??
                             (loggedInUser as SportsMember)?.DivisionId ??
                             (loggedInUser as Officials)?.DivisionId;

            var divisions = await _context.Divisions
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            ViewData["Divisions"] = divisions.Select(c => new SelectListItem
            {
                Value = c.DivisionId.ToString(),
                Text = c.DivisionName,
                Selected = Input.SelectedDivisionId == c.DivisionId 
            }).ToList();

            if (Input.SelectedDivisionId == 0 && divisions.Any())
            {
                Input.SelectedDivisionId = divisions.First().DivisionId; 
            }
        }



        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {

            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var loggedInUser = await _userManager.GetUserAsync(User);

            var divisionId = (loggedInUser as ClubAdministrator)?.DivisionId ??
                             (loggedInUser as ClubManager)?.DivisionId ??
                             (loggedInUser as Player)?.DivisionId ??
                             (loggedInUser as SportsMember)?.DivisionId ??
                             (loggedInUser as Officials)?.DivisionId;

            if (ModelState.IsValid)
            {
                var existingUserByPhoneNumber = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);
                if (existingUserByPhoneNumber != null)
                {
                    ModelState.AddModelError("Input.PhoneNumber", "An account with this phone number already exists.");

                    await _requestLogService.LogFailedRequest("Failed to create account due to phone number existence in this system.", StatusCodes.Status500InternalServerError);

                    return Page();
                }


                if (Input.SelectedDivisionId == null)
                {
                    ModelState.AddModelError(string.Empty, "Club must be selected.");
                    return Page();
                }

                var existingUserByEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Input.Email", "An account with this email address already exists.");

                    await _requestLogService.LogFailedRequest("Failed to create account due to email address existence in this system.", StatusCodes.Status500InternalServerError);

                    return Page();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = new DivisionManager
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    DateOfBirth = Input.DateOfBirth,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    CreatedBy = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedBy = userId,
                    ModifiedDateTime = DateTime.Now,
                    DivisionId= Input.SelectedDivisionId,
                    IsActive = true,
                    IsSuspended = false,
                    IsFirstTimeLogin = true
                };

                if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
                {
                    var playerProfilePicturePath = await _fileUploadService.UploadFileAsync(Input.ProfilePicture);
                    user.ProfilePicture = playerProfilePicturePath;
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                string randomPassword = _passwordGenerator.GenerateRandomPassword();
                var result = await _userManager.CreateAsync(user, randomPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _requestLogService.LogSuceededRequest("Account created susscessful.", StatusCodes.Status200OK);

                    await _userManager.AddToRoleAsync(user, "Division Manager");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);


                    var division = await _context.Divisions
                        .Where(c => c.DivisionId == Input.SelectedDivisionId)
                        .FirstOrDefaultAsync();

                    var userSubscription = new Subscription
                    {
                        UserId = user.Id,
                        Amount = 0,
                        SubscriptionPlan = SubscriptionPlan.Basic,
                        SubscriptionStatus = SubscriptionStatus.Active,
                    };

                    _context.Add(userSubscription);
                    _context.SaveChanges();

                    string accountCreationEmailBody = $"Hello {user.FirstName} {user.LastName},<br><br>";
                    accountCreationEmailBody += $"Welcome to Diski360!<br><br>";
                    accountCreationEmailBody += $"Please note that you have been added as a {division.DivisionName} manager.<br><br>";
                    accountCreationEmailBody += $"Below are your login credentials:<br><br>";
                    accountCreationEmailBody += $"Email: {user.Email}<br>";
                    accountCreationEmailBody += $"Password: {randomPassword}<br><br>";
                    accountCreationEmailBody += $"Please note that we have sent you two emails, including this one. You need to open the other email to confirm your email address before you can log into the system.<br><br>";
                    accountCreationEmailBody += $"Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Welcome to Diski360", accountCreationEmailBody, "Diski 360"));


                    string emailConfirmationEmailBody = $"Hello {user.FirstName},<br><br>";
                    emailConfirmationEmailBody += $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>";
                    emailConfirmationEmailBody += $"Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Confirm Your Email Address", emailConfirmationEmailBody, "Diski 360"));

                    await _activityLogger.Log($"Added {Input.FirstName} {Input.LastName} as {division.DivisionName} manager", userId);


                    TempData["Message"] = $"{user.FirstName} {user.LastName}  has been successfully added as {division.DivisionName} manager";

                    return RedirectToAction("SportManagers", "Users");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var roles = await _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                        .ToListAsync();
            ViewData["Roles"] = roles;

            var divisions = await _context.Divisions.ToListAsync();
            ViewData["Divisions"] = divisions.Select(c => new SelectListItem
            {
                Value = c.DivisionId.ToString(), 
                Text = c.DivisionName,
                Selected = Input.SelectedDivisionId == c.DivisionId 
            }).ToList();

            await _requestLogService.LogFailedRequest("Failed to create account.", StatusCodes.Status500InternalServerError);

            return Page();
        }

        private IUserEmailStore<UserBaseModel> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<UserBaseModel>)_userStore;
        }
    }
}
