/*using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;

namespace MyField.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;

        public InvoicesController(Ksans_SportsDbContext context,
               UserManager<UserBaseModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyInvoices()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");

            }
                var myInvoices = await _context.Invoices
                .Where(mo => mo.Transfer.CustomerClub.ClubId == clubId || mo.Fine.ClubId == clubId || mo.Fine.OffenderId == user.Id)
                .Include (s => s.Fine)
                .Include(s => s.Transfer)
                .ToListAsync(); 

            return View(myInvoices);
        }
    }
}
*/