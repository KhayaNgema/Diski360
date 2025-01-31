using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;

namespace MyField.Controllers
{
    public class CookiePreferencesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;

        public CookiePreferencesController(Ksans_SportsDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userId = User.Identity.Name;
            var preferences = await _context.CookiePreferences
                .FirstOrDefaultAsync(p => p.UserId == userId)
                ?? new CookiePreferences { UserId = userId };

            return View(preferences);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SavePreferences([FromBody] CookiePreferences preferences)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var userId = User.Identity.Name;

                var existingPreferences = await _context.CookiePreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingPreferences != null)
                {
                    existingPreferences.PerformanceCookies = preferences.PerformanceCookies;
                    existingPreferences.FunctionalityCookies = preferences.FunctionalityCookies;
                    existingPreferences.TargetingCookies = preferences.TargetingCookies;
                    _context.Update(existingPreferences);
                }
                else
                {
                    preferences.UserId = userId;
                    _context.CookiePreferences.Add(preferences);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPreferences()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userId = User.Identity.Name;
            var preferences = await _context.CookiePreferences
                .FirstOrDefaultAsync(p => p.UserId == userId)
                ?? new CookiePreferences { UserId = userId };

            return Json(preferences);
        }
    }
}
