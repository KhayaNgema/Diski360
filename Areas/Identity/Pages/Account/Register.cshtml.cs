using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;

namespace MyField.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<UserBaseModel> _signInManager;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IUserStore<UserBaseModel> _userStore;
        private readonly IUserEmailStore<UserBaseModel> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly FileUploadService _fileUploadService;
        private readonly IActivityLogger _activityLogger;
        private readonly Ksans_SportsDbContext _context;
        private readonly EmailService _emailService;
        private readonly RequestLogService _requestLogService;

        public RegisterModel(
            UserManager<UserBaseModel> userManager,
            IUserStore<UserBaseModel> userStore,
            SignInManager<UserBaseModel> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            FileUploadService fileUploadService,
            IActivityLogger activityLogger,
            Ksans_SportsDbContext context,
            EmailService emailService,
            RequestLogService requestLogService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _fileUploadService = fileUploadService;
            _activityLogger = activityLogger;
            _context = context;
            _emailService = emailService;
            _requestLogService = requestLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {

            [Required(ErrorMessage = "You must accept the terms and conditions.")]
            [Display(Name = "Accept Terms and Conditions")]
            [ValidateNever]
            public bool AcceptTerms { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Phone number")]
            public string PhoneNumber{ get; set; }

            [Required]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Date of birth")]
            public DateTime DateOfBirth { get; set; }

            [Display(Name = "Profile picture")]
            public IFormFile? ProfilePicture { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                if (!Input.AcceptTerms)
                {
                    ModelState.AddModelError("Input.AcceptTerms", "You must accept our terms and conditions to continue with registration.");

                    await _requestLogService.LogFailedRequest("Failed to register due to unaccepted terms and conditions.", StatusCodes.Status500InternalServerError);

                    return Page();
                }

                var existingUserByPhoneNumber = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.PhoneNumber);
                if (existingUserByPhoneNumber != null)
                {
                    ModelState.AddModelError("Input.PhoneNumber", "An account with this phone number already exists.");

                    await _requestLogService.LogFailedRequest("Failed to register due to cellphone number existence in the system.", StatusCodes.Status500InternalServerError);

                    return Page();
                }

                var existingUserByEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Input.Email", "An account with this email address already exists.");

                    await _requestLogService.LogFailedRequest("Failed to register due to email address existence in the system.", StatusCodes.Status500InternalServerError);

                    return Page();
                }

                var user = new Fan
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    DateOfBirth = Input.DateOfBirth,
                    IsActive = true,
                    IsSuspended = false,
                    PhoneNumber = Input.PhoneNumber
                };

                if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
                {
                    var playerProfilePicturePath = await _fileUploadService.UploadFileAsync(Input.ProfilePicture);
                    user.ProfilePicture = playerProfilePicturePath;
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme);

                    var emailConfirmationBody = $"Hello {user.FirstName},<br><br>" +
                        "Thank you for registering. Please confirm your email address by " +
                        $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>" +
                        "Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Confirm your email", emailConfirmationBody, "Diski 360"));

                    var newAggrement = new TermsAggreement
                    {
                        IsAggreed = true,
                        Id = userId,
                        AggreementTimeStamp = DateTime.Now,
                        UserBaseModel = user,
                    };

                    var userSubscription = new Subscription
                    {
                        UserId = user.Id,
                        Amount = 0,
                        SubscriptionPlan = SubscriptionPlan.Basic,
                        SubscriptionStatus = SubscriptionStatus.Active,
                    };

                    _context.Add(userSubscription);
                    _context.Add(newAggrement);
                    await _context.SaveChangesAsync();

                    await _requestLogService.LogSuceededRequest("Account created successful.", StatusCodes.Status200OK);

                    return RedirectToAction("AccountCreatedSuccessfully", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

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
