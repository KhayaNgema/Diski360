// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;

namespace MyField.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly SignInManager<UserBaseModel> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly Ksans_SportsDbContext _context;
        private readonly EmailService _emailService;
        private readonly IActivityLogger _activityLogger;


        public ChangePasswordModel(
            UserManager<UserBaseModel> userManager,
            SignInManager<UserBaseModel> signInManager,
            ILogger<ChangePasswordModel> logger,
            Ksans_SportsDbContext context,
            EmailService emailService,
            IActivityLogger activityLogger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _activityLogger = activityLogger;
        }

        /// <summary>
        ///     This API supports the (link unavailable) Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the (link unavailable) Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the (link unavailable) Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the (link unavailable) Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Old password")]
            public string OldPassword { get; set; }

            /// <summary>
            ///     This API supports the (link unavailable) Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            /// <summary>
            ///     This API supports the (link unavailable) Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            string emailBody = $@"
                    Hi {user.FirstName} {user.LastName},<br/><br/>
                    Your password has been changed successfully  
                    and now you will be required to use your new password for aunthetication.<br/><br/>
                    If you did not request this change, please contact our support team immediately.<br/><br>
                    Kind regards,<br/>
                    Diski360 Support Team
            ";

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Password change successful", emailBody, "Diski 360"));

            await _activityLogger.Log($"Changed password", user.Id);

            TempData["Message"] = $"You have successfully changed your password";

            return RedirectToAction("PasswordAndSecurity", "Users");
        }

    }
}