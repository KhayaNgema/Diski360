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
    public class RegisterClubPlayersModel : PageModel
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


        public RegisterClubPlayersModel(
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

            [Required(ErrorMessage = "The Profile picture field is required.")]
            [Display(Name = "Profile picture")]
            public IFormFile ProfilePicture { get; set; }


            [Display(Name = "Player card")]
            public IFormFile? PlayerCard{ get; set; }

            [Required]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Position")]
            public Position Position { get; set; }

            [Required]
            [Display(Name = "Jersey number")]
            public int JerseyNumber { get; set; }

            [Required]
            [Display(Name = "Market value")]
            public decimal MarketValue { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Fetch roles from the database
            var roles = await _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToListAsync();

            // Pass the SelectList to ViewData
            ViewData["Roles"] = roles;

            ViewData["Positions"] = Enum.GetValues(typeof(Position))
                                   .Cast<Position>()
                                   .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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

                var loggedInUser = await _userManager.GetUserAsync(User);

                var divisionId = (loggedInUser as ClubAdministrator)?.DivisionId ??
                                 (loggedInUser as ClubManager)?.DivisionId ??
                                 (loggedInUser as Player)?.DivisionId ??
                                 (loggedInUser as SportsMember)?.DivisionId ??
                                 (loggedInUser as Officials)?.DivisionId;

                var windowPeriod = await _context.TransferPeriod
                    .Where(wp => wp.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if(windowPeriod.IsOpened == false)
                {
                    ModelState.AddModelError(string.Empty, "You can't onboard a new player when a transfer period is closed. Please try again next round.");

                    await _requestLogService.LogFailedRequest("Failed to onboard a player due to transfer period closed.", StatusCodes.Status500InternalServerError);

                    return Page();
                }

                var user = new Player
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    DateOfBirth = Input.DateOfBirth,
                    Email = Input.Email,
                    JerseyNumber = Input.JerseyNumber,
                    PhoneNumber = Input.PhoneNumber,
                    CreatedBy = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedBy = userId,
                    ModifiedDateTime = DateTime.Now,
                    Position = Input.Position,
                    MarketValue = Input.MarketValue,
                    IsActive = true,
                    IsSuspended = false,
                    IsFirstTimeLogin = true,
                    DivisionId = divisionId
                };

                var clubAdmin = await _context.ClubAdministrator
                      .Where(c => c.Id == loggedInUser.Id)
                      .Include(c => c.Club)
                      .FirstOrDefaultAsync();


                if (clubAdmin != null)
                {
                    user.ClubId = clubAdmin.ClubId;
                }
                else
                {
                    string errorMessage = "Only club administrators can be able to add players into this system";

                    return RedirectToAction("ErrorPage", new { errorMessage = errorMessage });
                }



                if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
                {
                    var playerProfilePicturePath = await _fileUploadService.UploadFileAsync(Input.ProfilePicture);
                    user.ProfilePicture = playerProfilePicturePath;
                }



                if (Input.PlayerCard != null && Input.PlayerCard.Length > 0)
                {
                    var playerCardPicturePath = await _fileUploadService.UploadFileAsync(Input.PlayerCard);
                    user.PlayerCard= playerCardPicturePath;
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                string randomPassword = _passwordGenerator.GenerateRandomPassword();
                var result = await _userManager.CreateAsync(user, randomPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _requestLogService.LogSuceededRequest("Account created susscessful.", StatusCodes.Status200OK);

                    await _userManager.AddToRoleAsync(user, "Player");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string accountCreationEmailBody = $"Hello {user.FirstName},<br><br>";
                    accountCreationEmailBody += $"Welcome to {user.Club.ClubName}!<br><br>";
                    accountCreationEmailBody += $"You have been successfully added as {user.Club.ClubName} player. Below are your login credentials:<br><br>";
                    accountCreationEmailBody += $"Email: {user.Email}<br>";
                    accountCreationEmailBody += $"Password: {randomPassword}<br><br>";
                    accountCreationEmailBody += $"Please note that we have sent you two emails, including this one. You need to open the other email to confirm your email address before you can log into the system.<br><br>";
                    accountCreationEmailBody += $"Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, $"Welcome to {user.Club.ClubName} ", accountCreationEmailBody, "Diski 360"));

                    string emailConfirmationEmailBody = $"Hello {user.FirstName},<br><br>";
                    emailConfirmationEmailBody += $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>";
                    emailConfirmationEmailBody += $"Thank you!";

                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Confirm Your Email Address", emailConfirmationEmailBody, "Diski 360"));

                    var club = await _context.Club
                        .Where(c => c.ClubId == clubAdmin.ClubId)
                        .FirstOrDefaultAsync();

                    var currentSeason = await _context.League
                        .Where(c => c.IsCurrent)
                        .FirstOrDefaultAsync();

                    var newPlayerPerfomanceReport = new PlayerPerformanceReport
                    {
                        PlayerId = user.Id,
                        AppearancesCount = 0,
                        GoalsScoredCount = 0,
                        AssistsCount = 0,
                        YellowCardCount = 0,
                        RedCardCount = 0,
                        LeagueId = currentSeason.LeagueId
                    };

                   

                    _context.Add(newPlayerPerfomanceReport);

                    var newTopScore = new TopScore
                    {
                        DivisionId = divisionId,
                        LeagueId = currentSeason.LeagueId,
                        NumberOfGoals = 0,
                        PlayerId = user.Id,
                        
                    };

                    _context.Add(newTopScore);

                    var newTopAssist = new TopAssist
                    {
                        DivisionId = divisionId,
                        LeagueId = currentSeason.LeagueId,
                        NumberOfAssists = 0,
                        PlayerId = user.Id,

                    };

                    _context.Add(newTopAssist);

                    await _context.SaveChangesAsync();

                    var userSubscription = new Subscription
                    {
                        UserId = user.Id,
                        Amount = 0,
                        SubscriptionPlan = SubscriptionPlan.Basic,
                        SubscriptionStatus = SubscriptionStatus.Active,
                    };

                    _context.Add(userSubscription);
                    _context.SaveChanges();

                    await _activityLogger.Log($"Added {Input.FirstName} {Input.LastName} as {club?.ClubName} new player ", userId);
                        
                    TempData["Message"] = $"{user.FirstName} {user.LastName}  has been successfully added as your new player";
                    return RedirectToAction("MyClubPlayers", "Users");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewData["Positions"] = Enum.GetValues(typeof(Position))
                                 .Cast<Position>()
                                 .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            var roles = await _roleManager.Roles
                        .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                        .ToListAsync();
            ViewData["Roles"] = roles;

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
