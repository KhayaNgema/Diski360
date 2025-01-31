using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;

namespace MyField.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IEncryptionService _encryptionService;

        public ActivityLogsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            IEncryptionService encryptionService)
        {
            _context = context;
            _userManager = userManager;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> LogDetails(string logId)
        {
            var decryptedLogId = _encryptionService.DecryptToInt(logId);

            if (logId == null) 
            {
                return NotFound();
            }

            var activityLog = await _context.ActivityLogs
                .Where(al => al.ActivityLogId == decryptedLogId)
                .Include(al => al.UserBaseModel)
                .Include(al => al.DeviceInfo)
                .FirstOrDefaultAsync();

            return View(activityLog);
        }

        public async Task<IActionResult> AuditTrial()
        {

            var audits = await _context.ActivityLogs
                .Include(a => a.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(audits);
        }

        [Authorize(Roles =("Club Administrator"))]
        public async Task<IActionResult> MyPlayersActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
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
            else
            {

            }

            var players = await _context.Player
                .Where(p => p.ClubId == clubId)
                .ToListAsync();

            var playerLogs = new List<ActivityLog>();

            foreach (var player in players)
            {
                var logs = await _context.ActivityLogs
                    .Where(a => a.UserId == player.Id)
                    .Include(a => a.UserBaseModel)
                    .Include(a => a.DeviceInfo)
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                playerLogs.AddRange(logs);
            }

            return View(playerLogs);
        }


        [Authorize]
        public async Task<IActionResult> MyActivityLogs()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound();
                }

                var myActivityLogs = await _context.ActivityLogs
                    .Where(a => a.UserId == user.Id)
                    .Include(a => a.DeviceInfo)
                    .Include(a => a.UserBaseModel)
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                return View(myActivityLogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyManagersActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var manager = await _context.ClubManager
                .Where(m => m.ClubId == clubId)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var managerLogs = await _context.ActivityLogs
                .Where(a => a.UserId == manager.Id)
                .Include(a => a.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(managerLogs);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> PersonnelLogs()
        {
            return View();
        }


        [Authorize(Roles = ("Fans Administrator"))]
        public async Task<IActionResult> FansActivityLogs()
        {
            var fansActivityLogs = await _context.ActivityLogs
                .Where(log => !_context.Roles
                    .Any(r => _context.UserRoles
                        .Any(ur => ur.UserId == log.UserId && ur.RoleId == r.Id)))
                .Include(log => log.UserBaseModel)
                .Include(log => log.DeviceInfo)
                .OrderByDescending( log => log.Timestamp)
                .ToListAsync();

            return View(fansActivityLogs);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> ClubAdministratorsActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            var clubAdministrators = await _context.ClubAdministrator
                .Where(c => c.DivisionId == divisionId)
                .Include(c => c.Club)
                .ToListAsync();

            if (clubAdministrators == null || clubAdministrators.Count == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            List<ActivityLogViewModel> allClubAdminLogs = new List<ActivityLogViewModel>();

            foreach (var clubAdministrator in clubAdministrators)
            {
                var clubAdministratorLogs = await _context.ActivityLogs
                    .Where(a => a.UserId == clubAdministrator.Id)
                    .Include(a => a.UserBaseModel)
                    .Include(a => a.DeviceInfo)
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                foreach (var log in clubAdministratorLogs)
                {
                    var viewModel = new ActivityLogViewModel
                    {
                        ActivityLogId = log.ActivityLogId,
                        FirstName = log.UserBaseModel?.FirstName,
                        LastName = log.UserBaseModel?.LastName,
                        ClubName = clubAdministrator.Club?.ClubName,
                        Activity = log.Activity,
                        Timestamp = log.Timestamp,
                        DeviceDetails = log.DeviceInfo != null ? $"{log.DeviceInfo.DeviceName}, {log.DeviceInfo.OSName}, {log.DeviceInfo.OSVersion}, {log.DeviceInfo.DeviceModel}, {log.DeviceInfo.IpAddress}" : "",
                        BrowserDetails = log.DeviceInfo != null ? $"{log.DeviceInfo.Browser}, {log.DeviceInfo.BrowserVersion}, {log.DeviceInfo.DeviceModel}" : "",
                        LocationDetails = log.DeviceInfo != null ? $"{log.DeviceInfo.City}, {log.DeviceInfo.PostalCode}, {log.DeviceInfo.Region}, {log.DeviceInfo.Country}" : ""
                    };

                    allClubAdminLogs.Add(viewModel);
                }

                allClubAdminLogs = allClubAdminLogs.OrderByDescending(a => a.Timestamp).ToList();
            }

            return PartialView("_ClubAdministratorsActivityLogsPartial", allClubAdminLogs);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> ClubManagersActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            var clubManagers = await _context.ClubManager
                .Where(cm => cm.DivisionId == divisionId)
                .Include(c => c.Club)
                .ToListAsync();

            if (clubManagers == null || clubManagers.Count == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            List<ActivityLogViewModel> allClubManagerLogs = new List<ActivityLogViewModel>();

            foreach (var clubManager in clubManagers)
            {
                var clubManagerLogs = await _context.ActivityLogs
                    .Where(a => a.UserId == clubManager.Id)
                    .Include(a => a.UserBaseModel)
                    .Include(a => a.DeviceInfo)
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                var club = await _context.Club
                    .Where(c => c.ClubId == clubManager.ClubId)
                    .FirstOrDefaultAsync();

                foreach (var log in clubManagerLogs)
                {
                    var viewModel = new ActivityLogViewModel
                    {
                        ActivityLogId = log.ActivityLogId,
                        FirstName = log.UserBaseModel?.FirstName,
                        LastName = log.UserBaseModel?.LastName,
                        ClubName = club?.ClubName,
                        Activity = log.Activity,
                        Timestamp = log.Timestamp,
                        DeviceDetails = log.DeviceInfo != null ? $"{log.DeviceInfo.DeviceName}, {log.DeviceInfo.OSName}, {log.DeviceInfo.OSVersion}, {log.DeviceInfo.DeviceModel}, {log.DeviceInfo.IpAddress}" : "",
                        BrowserDetails = log.DeviceInfo != null ? $"{log.DeviceInfo.Browser}, {log.DeviceInfo.BrowserVersion}, {log.DeviceInfo.DeviceModel}" : "",
                        LocationDetails = log.DeviceInfo != null ? $"{log.DeviceInfo.City}, {log.DeviceInfo.PostalCode}, {log.DeviceInfo.Region}, {log.DeviceInfo.Country}" : ""
                    };

                    allClubManagerLogs.Add(viewModel);
                }
            }

            allClubManagerLogs = allClubManagerLogs.OrderByDescending(a => a.Timestamp).ToList();

            return PartialView("_ClubManagersActivityLogsPartial", allClubManagerLogs);
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<IActionResult> NewsAdministratorsActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var newsAdminRoleId = await _context.Roles
                .Where(r => r.Name == "News Administrator")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (newsAdminRoleId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var newsAdminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == newsAdminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var newsAdminsActivityLogs = await _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .OrderByDescending(a => a.Timestamp)
                .Where(a => newsAdminUserIds.Contains(a.UserId) &&
                            _context.SportMember.Any(sm => sm.Id == a.UserId && sm.DivisionId == divisionId))
                .ToListAsync();

            return PartialView("_NewsAdministratorsActivityLogsPartial", newsAdminsActivityLogs);
        }


        [Authorize(Roles = "Personnel Administrator")]
        public async Task<IActionResult> SportAdministratorsActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var sportAdminRoleId = await _context.Roles
                .Where(r => r.Name == "Sport Administrator")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (sportAdminRoleId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var sportAdminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == sportAdminRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var sportAdminsActivityLogs = await _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => sportAdminUserIds.Contains(a.UserId) &&
                            _context.SportMember.Any(sm => sm.Id == a.UserId && sm.DivisionId == divisionId))
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return PartialView("_SportAdministratorsActivityLogsPartial", sportAdminsActivityLogs);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> ManagersActivityLogs()
        {
            return View();
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<IActionResult> PlayersActivityLogs()
        {
            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubPlayers = await _context.Player
                .Include(p => p.Club)
                .Where(p => p.DivisionId == divisionId && !p.IsDeleted)
                .ToListAsync();

            if (clubPlayers == null || clubPlayers.Count == 0)
            {
                return RedirectToAction("Error", "Home");
            }

            var playerIds = clubPlayers.Select(p => p.Id).ToList();
            var playerActivityLogs = await _context.ActivityLogs
                .Where(a => playerIds.Contains(a.UserId))
                .Include(a => a.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            var allClubPlayerLogs = playerActivityLogs
                .Select(log => new ActivityLogViewModel
                {
                    ActivityLogId = log.ActivityLogId,
                    FirstName = log.UserBaseModel?.FirstName,
                    LastName = log.UserBaseModel?.LastName,
                    ClubName = clubPlayers.FirstOrDefault(p => p.Id == log.UserId)?.Club?.ClubName,
                    Activity = log.Activity,
                    Timestamp = log.Timestamp,
                    DeviceDetails = log.DeviceInfo != null
                        ? $"{log.DeviceInfo.DeviceName}, {log.DeviceInfo.OSName}, {log.DeviceInfo.OSVersion}, {log.DeviceInfo.DeviceModel}, {log.DeviceInfo.IpAddress}"
                        : "",
                    BrowserDetails = log.DeviceInfo != null
                        ? $"{log.DeviceInfo.Browser}, {log.DeviceInfo.BrowserVersion}, {log.DeviceInfo.DeviceModel}"
                        : "",
                    LocationDetails = log.DeviceInfo != null
                        ? $"{log.DeviceInfo.City}, {log.DeviceInfo.PostalCode}, {log.DeviceInfo.Region}, {log.DeviceInfo.Country}"
                        : ""
                })
                .OrderByDescending(vm => vm.Timestamp)
                .ToList();

            return PartialView("_PlayerActivityLogsPartial", allClubPlayerLogs);
        }


        [Authorize(Roles = "Personnel Administrator")]
        public IActionResult NewsUpdatersActivityLogs()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var newsUpdaterRoleId = _context.Roles
                .Where(r => r.Name == "News Updator")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(newsUpdaterRoleId))
            {
                return RedirectToAction("Error", "Home");
            }

            var newsUpdatersActivityLogs = _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => _context.UserRoles
                    .Where(ur => ur.RoleId == newsUpdaterRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(a.UserId) &&
                    _context.SportMember
                    .Where(sm => sm.Id == a.UserId && sm.DivisionId == divisionId)
                    .Any())
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return PartialView("_NewsUpdatersActivityLogsPartial", newsUpdatersActivityLogs);
        }


        [Authorize(Roles = "Personnel Administrator")]
        public IActionResult SportCoordinatorsActivityLogs()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var sportCoordinatorRoleId = _context.Roles
                .Where(r => r.Name == "Sport Coordinator")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(sportCoordinatorRoleId))
            {
                return RedirectToAction("Error", "Home");
            }

            var sportCoordinatorsActivityLogs = _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => _context.UserRoles
                    .Where(ur => ur.RoleId == sportCoordinatorRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(a.UserId) &&
                    _context.SportMember
                    .Where(sm => sm.Id == a.UserId && sm.DivisionId == divisionId)
                    .Any())
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return PartialView("_SportCoordinatorsActivityLogsPartial", sportCoordinatorsActivityLogs);
        }


        [Authorize(Roles = "Personnel Administrator")]
        public IActionResult OfficialsActivityLogs()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var officialRoleId = _context.Roles
                .Where(r => r.Name == "Official")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(officialRoleId))
            {
                return RedirectToAction("Error", "Home");
            }

            var officialsActivityLogs = _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => _context.UserRoles
                    .Where(ur => ur.RoleId == officialRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(a.UserId) &&
                    _context.SportMember
                    .Where(sm => sm.Id == a.UserId && sm.DivisionId == divisionId)
                    .Any())
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return PartialView("_OfficialsActivityLogsPartial", officialsActivityLogs);
        }


        [Authorize(Roles = "Division Manager")]
        public IActionResult PersonnelAdministratorsActivityLogs()
        {
            var user = _userManager.GetUserAsync(User).Result;

            var divisionId = (user as DivisionManager)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var personnelAdminRoleId = _context.Roles
                .Where(r => r.Name == "Personnel Administrator")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(personnelAdminRoleId))
            {
                return RedirectToAction("Error", "Home");
            }

            var personnelAdminsActivityLogs = _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => _context.UserRoles
                    .Where(ur => ur.RoleId == personnelAdminRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(a.UserId) &&
                    _context.SportMember
                    .Where(sm => sm.Id == a.UserId && sm.DivisionId == divisionId)
                    .Any())
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return View(personnelAdminsActivityLogs);
        }

        [Authorize(Roles = "Personnel Administrator")]
        public IActionResult SportManagersActivityLogs()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var sportManagerRoleId = _context.Roles
                .Where(r => r.Name == "Division Manager")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(sportManagerRoleId))
            {
                return RedirectToAction("Error", "Home");
            }

            var sportManagersActivityLogs = _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => _context.UserRoles
                    .Where(ur => ur.RoleId == sportManagerRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(a.UserId) &&
                    _context.DivisionManagers
                    .Where(sm => sm.Id == a.UserId && sm.DivisionId == divisionId)
                    .Any())
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return PartialView("_SportManagersActivityLogsPartial", sportManagersActivityLogs);
        }


        [Authorize(Roles = "Personnel Administrator")]
        public IActionResult FansAdministratorsActivityLogs()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var fansAdminRoleId = _context.Roles
                .Where(r => r.Name == "Fans Administrator")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(fansAdminRoleId))
            {
                return RedirectToAction("Error", "Home");
            }

            var fansAdminsActivityLogs = _context.ActivityLogs
                .Include(n => n.UserBaseModel)
                .Include(a => a.DeviceInfo)
                .Where(a => _context.UserRoles
                    .Where(ur => ur.RoleId == fansAdminRoleId)
                    .Select(ur => ur.UserId)
                    .Contains(a.UserId) &&
                    _context.SportMember
                    .Where(sm => sm.Id == a.UserId && sm.DivisionId == divisionId)
                    .Any())
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return PartialView("_FansAdministratorsActivityLogsPartial", fansAdminsActivityLogs);
        }


    }
}
