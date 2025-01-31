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
    public class RegisterSystemAdministratorModel : PageModel
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
        private readonly IActivityLogger _activityLogger;
        private readonly Ksans_SportsDbContext _context;
        private readonly RequestLogService _requestLogService;

        public RegisterSystemAdministratorModel(
            UserManager<UserBaseModel> userManager,
            IUserStore<UserBaseModel> userStore,
            SignInManager<UserBaseModel> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            FileUploadService fileUploadService,
            RoleManager<IdentityRole> roleManager,
            RandomPasswordGeneratorService passwordGenerator,
            EmailService emailService,
            IActivityLogger activityLogger,
            Ksans_SportsDbContext context,
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
            _activityLogger = activityLogger;
            _context = context; 
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

            [Display(Name = "Division")]
            public int? DivisionId { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var divisions = await _context.Divisions
                .Select(r => new { r.DivisionId, r.DivisionName })
                .ToListAsync();

            ViewData["Divisions"] = new SelectList(divisions, "DivisionId", "DivisionName");
        }




        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var userLoggedIn = await _userManager.GetUserAsync(User);

            var loggedInUser = await _context.SportMember
                .Where(lu => lu.Id == userLoggedIn.Id && lu.Email == userLoggedIn.Email)
                .FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
              
                var existingUserByPhoneNumber = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);
                if (existingUserByPhoneNumber != null)
                {
                    ModelState.AddModelError("Input.PhoneNumber", "An account with this phone number already exists.");

                    await _requestLogService.LogFailedRequest("Failed to create account due to phone number existence in this system.", StatusCodes.Status500InternalServerError);

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


                var divisionId = (userLoggedIn as ClubAdministrator)?.DivisionId ??
                                 (userLoggedIn as ClubManager)?.DivisionId ??
                                 (userLoggedIn as Player)?.DivisionId ??
                                 (userLoggedIn as SportsMember)?.DivisionId ??
                                 (userLoggedIn as Officials)?.DivisionId ??
                                 (userLoggedIn as DivisionManager)?.DivisionId;

                var user = new SportsMember
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
                    IsActive = true,
                    IsSuspended = false,
                    IsFirstTimeLogin = true,
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

                    var userSubscription = new Subscription
                    {
                        UserId = user.Id,
                        Amount = 0,
                        SubscriptionPlan = SubscriptionPlan.Basic,
                        SubscriptionStatus = SubscriptionStatus.Active,
                    };

                    _context.Add(userSubscription);
                    _context.SaveChanges();

                    await _userManager.AddToRoleAsync(user, "System Administrator");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string accountCreationEmailBody = $"Hello {user.FirstName},<br><br>";
                    accountCreationEmailBody += $"Your account has been successfully created. Below are your login credentials:<br><br>";
                    accountCreationEmailBody += $"Email: {user.Email}<br>";
                    accountCreationEmailBody += $"Password: {randomPassword}<br><br>";
                    accountCreationEmailBody += $"Please note that we have sent you two emails, including this one. You need to open the other email to confirm your email address before you can log into the system.<br><br>";
                    accountCreationEmailBody += $"Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Welcome to Diski360", accountCreationEmailBody, "Diski 360"));

                    string emailConfirmationEmailBody = $"Hello {user.FirstName},<br><br>";
                    emailConfirmationEmailBody += $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>";
                    emailConfirmationEmailBody += $"Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Confirm Your Email Address", emailConfirmationEmailBody, "Diski 360"));

                    await _activityLogger.Log($"Added {user.FirstName} {user.LastName} as a system administrator", userId);

                    TempData["Message"] = $"{Input.FirstName} {Input.LastName}  has been successfully added as a new system administrator";

                    var userRole = await _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                             ur => ur.RoleId,
                             r => r.Id,
                             (ur, r) => r.Name)
                        .FirstOrDefaultAsync();

                    if (userRole == "Club Administrator")
                    {
                        return RedirectToAction("ClubAdministrators", "Users");
                    }
                    else if (userRole == "Sport Administrator")
                    {
                        return RedirectToAction("SportAdministrators", "Users");
                    }
                    else if (userRole == "Division Manager")
                    {
                        return RedirectToAction("ManageDivisionManagers", "Divisions");
                    }
                    else if (userRole == "Sport Coordinator")
                    {
                        return RedirectToAction("SportCoordinators", "Users");
                    }
                    else if (userRole == "Official")
                    {
                        return RedirectToAction("Officials", "Users");
                    }
                    else if (userRole == "News Administrator")
                    {
                        return RedirectToAction("NewsAdministrators", "Users");
                    }
                    else if (userRole == "News Updator")
                    {
                        return RedirectToAction("NewsUpdaters", "Users");
                    }
                    else if (userRole == "Fans Administrator")
                    {
                        return RedirectToAction("FansAdministrators", "Users");
                    }
                    else if (userRole == "Personnel Administrator")
                    {
                        return RedirectToAction("PersonnelAdministrators", "Users");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var rolesQuery = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name });

            var userRoles = await _userManager.GetRolesAsync(userLoggedIn);

            var currentlyLoggedInUser = await _context.SportMember
                .Where(lu => lu.Id == loggedInUser.Id && lu.Email == loggedInUser.Email)
                .FirstOrDefaultAsync();

            var userDivisionId = loggedInUser.DivisionId;

            if (userRoles.Contains("Personnel Administrator") || userRoles.Contains("Division Manager") && userDivisionId == null)
            {
                rolesQuery = rolesQuery.Where(r => r.Value != "System Administrator");
            }

            var roles = await rolesQuery.ToListAsync();

            ViewData["Roles"] = roles;

            var divisions = await _context.Divisions
                .Select(r => new { r.DivisionId, r.DivisionName })
                .ToListAsync();

            ViewData["Divisions"] = new SelectList(divisions, "DivisionId", "DivisionName");

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
