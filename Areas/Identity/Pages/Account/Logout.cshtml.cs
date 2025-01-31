// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;

namespace MyField.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<UserBaseModel> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IActivityLogger _activityLogger;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly RequestLogService _requestLogService;

        public LogoutModel(SignInManager<UserBaseModel> signInManager, 
            ILogger<LogoutModel> logger,
            IActivityLogger activityLogger,
            UserManager<UserBaseModel> userManager,
            RequestLogService requestLogService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _activityLogger = activityLogger;
            _userManager = userManager;
            _requestLogService = requestLogService;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                var user = await _userManager.GetUserAsync(User);
                await _activityLogger.Log("Logged out", user.Id);

                await _requestLogService.LogSuceededRequest("Logged out successful.", StatusCodes.Status200OK);

                return LocalRedirect(returnUrl);
            }
            else
            {
                await _requestLogService.LogFailedRequest("Failed to log out.", StatusCodes.Status500InternalServerError);

                return RedirectToPage();
            }
        }
    }
}
