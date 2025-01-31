using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;

namespace MyField.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly EmailService _emailService;
        private readonly IActivityLogger _activityLogger;

        public ResetPasswordModel(UserManager<UserBaseModel> userManager, 
            EmailService emailService,
            IActivityLogger activityLogger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _activityLogger = activityLogger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Display(Name = "New password")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError("Input.Email", "The user with the email address you provided does not exist.");
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

            if (result.Succeeded)
            {
                string emailBody = $@"
                    Hi {user.FirstName} {user.LastName},<br/><br/>
                    Your password has been successfully reset.<br/><br/>
                    If you did not request this change, please contact our support team immediately.<br/><br>
                    Kind regards,<br/>
                    Diski360 Support Team";

                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Password reset sccessful", emailBody, "Diski 360"));

                await _activityLogger.Log($"Forgot and reset password", user.Id);

                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
