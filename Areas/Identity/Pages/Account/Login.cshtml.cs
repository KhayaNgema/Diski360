// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MyField.Models;
using MyField.Interfaces;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Services;

namespace MyField.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<UserBaseModel> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IActivityLogger _activityLogger;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Ksans_SportsDbContext _context;
        private readonly RequestLogService _requestLogService;

        public LoginModel(SignInManager<UserBaseModel> signInManager,
            ILogger<LoginModel> logger,
            IActivityLogger activityLogger,
            UserManager<UserBaseModel> userManager,
            RoleManager<IdentityRole> roleManager,
            Ksans_SportsDbContext context,
            RequestLogService requestLogService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _activityLogger = activityLogger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _requestLogService = requestLogService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "Email or Phone")]
            public string EmailOrPhone { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var isEmail = new EmailAddressAttribute().IsValid(Input.EmailOrPhone);

                var user = isEmail ?
                            await _userManager.FindByEmailAsync(Input.EmailOrPhone) :
                            await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.EmailOrPhone);

                if (user != null)
                {
                    if (user.IsDeleted)
                    {
                        ModelState.AddModelError(string.Empty, "Your account is deleted and you don't have access to this system anymore.");
                        await _requestLogService.LogFailedRequest("Failed to log in due to deleted account status.", StatusCodes.Status401Unauthorized);
                    }
                    else if (user.IsSuspended)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has been suspended. Please contact your system administrator.");
                        await _requestLogService.LogFailedRequest("Failed to log in due to suspended account status.", StatusCodes.Status401Unauthorized);
                    }
                    else if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact your system administrator.");
                        await _requestLogService.LogFailedRequest("Failed to log in due to deactivated account status.", StatusCodes.Status401Unauthorized);
                    }
                    else if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "Your email address is not verified. Please verify it and try again.");
                        await _requestLogService.LogFailedRequest("Failed to log in due to unverified account status.", StatusCodes.Status401Unauthorized);
                    }
                    else
                    {
                        var divisionId = (user as ClubAdministrator)?.DivisionId ??
                                         (user as ClubManager)?.DivisionId ??
                                         (user as Player)?.DivisionId ??
                                         (user as SportsMember)?.DivisionId ??
                                         (user as Officials)?.DivisionId ??
                                         (user as DivisionManager)?.DivisionId;

                        if (divisionId != null)
                        {
                            var division = await _context.Divisions
                                .Where(d => d.DivisionId == divisionId)
                                .FirstOrDefaultAsync();

                            if (division != null)
                            {
                                if (!division.IsActive && !division.HasPaid)
                                {
                                    ModelState.AddModelError(string.Empty, "Your division has been deactivated due to outstanding payment. Please contact your division manager.");
                                }
                                else if (!division.IsActive && division.HasPaid)
                                {
                                    ModelState.AddModelError(string.Empty, "Your division has been deactivated. Please contact your division manager.");
                                }
                                else if (division.IsSuspended)
                                {
                                    ModelState.AddModelError(string.Empty, "Your division has been suspended. Please contact your division manager.");
                                }
                                else if (division.IsActive && division.HasPaid)
                                {

                                    var clubId = (user as ClubAdministrator)?.ClubId ??
                                                     (user as ClubManager)?.ClubId ??
                                                     (user as Player)?.ClubId;

                                    var club = await _context.Club
                                        .Where(c => c.ClubId == clubId)
                                        .FirstOrDefaultAsync();

                                    if(clubId != null)
                                    {
                                        if (club.IsSuspended)
                                        {
                                            ModelState.AddModelError(string.Empty, "Your club has been suspended due to an ongoing case or fraudulent conduct. Please contact your club administrator.");
                                        }
                                        else
                                        {
                                            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                                            if (result.Succeeded)
                                            {
                                                await _activityLogger.Log("Logged in", user.Id);
                                                await _requestLogService.LogSuceededRequest("Successfully logged into the system", StatusCodes.Status200OK);
                                                return LocalRedirect(returnUrl);
                                            }
                                            else
                                            {
                                                ModelState.AddModelError(string.Empty, "Incorrect email or phone number or password.");
                                                await _requestLogService.LogFailedRequest("Failed to log in due to incorrect email or password.", StatusCodes.Status406NotAcceptable);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                                        if (result.Succeeded)
                                        {
                                            await _activityLogger.Log("Logged in", user.Id);
                                            await _requestLogService.LogSuceededRequest("Successfully logged into the system", StatusCodes.Status200OK);
                                            return LocalRedirect(returnUrl);
                                        }
                                        else
                                        {
                                            ModelState.AddModelError(string.Empty, "Incorrect email or phone number or password.");
                                            await _requestLogService.LogFailedRequest("Failed to log in due to incorrect email or password.", StatusCodes.Status406NotAcceptable);
                                        }
                                    }
                                   
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "The assigned division could not be found.");
                            }
                        }
                        else
                        {
                           
                            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                            if (result.Succeeded)
                            {
                                await _activityLogger.Log("Logged in", user.Id);
                                await _requestLogService.LogSuceededRequest("Successfully logged into the system", StatusCodes.Status200OK);
                                return LocalRedirect(returnUrl);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Incorrect email or phone number or password.");
                                await _requestLogService.LogFailedRequest("Failed to log in due to incorrect email or password.", StatusCodes.Status406NotAcceptable);
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User account does not exist in this system.");
                    await _requestLogService.LogFailedRequest("Failed to log in due to account non-existence in this system.", StatusCodes.Status404NotFound);
                }
            }

            return Page();
        }



    }
}
