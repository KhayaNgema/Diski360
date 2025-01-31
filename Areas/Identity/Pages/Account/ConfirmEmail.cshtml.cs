using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MyField.Models;

namespace MyField.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<UserBaseModel> _userManager;

        public ConfirmEmailModel(UserManager<UserBaseModel> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                StatusMessage = "Error: Invalid confirmation link.";
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = $"Error: Unable to load user with ID '{userId}'.";
                return NotFound(StatusMessage);
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);

                if (result.Succeeded)
                {
                    StatusMessage = "Thank you for confirming your email.";
                }
                else
                {
                    StatusMessage = "Error confirming your email.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: An unexpected error occurred while confirming your email. Details: {ex.Message}";
            }

            return Page();
        }

    }
}
