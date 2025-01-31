using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;
using NuGet.Packaging;

namespace MyField.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly RequestLogService _requestLogService;
        public MeetingsController(Ksans_SportsDbContext context,
              UserManager<UserBaseModel> userManager,
              IActivityLogger activityLogger,
              EmailService emailService,
              RequestLogService requestLogService)
        {
            _userManager = userManager;
            _context = context;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _requestLogService = requestLogService;
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> Meetings()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId ??
                             (user as SportsMember)?.DivisionId ??
                             (user as Officials)?.DivisionId;

            IQueryable<Meeting> meetingsQuery = _context.Meeting
                .Include(m => m.CreatedBy)
                .Include(m => m.ModifiedBy)
                .OrderByDescending(m => m.CreatedDateTime)
                .Where(m => m.DivisionId == divisionId);

            if (User.IsInRole("Sport Coordinator"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Sport_Coordinators
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Club Administrator"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Club_Administrators
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Club Manager"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Club_Managers
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Player"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Players
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Sport Administrator"))
            {
                meetingsQuery = meetingsQuery;
            }
            else if (User.IsInRole("News Updator"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.News_Updaters
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Official"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Officials
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("News Administrator"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.News_Administrators
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Fans Administrator"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Fans_Administrators
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Personnel Administrator"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Personnel_Administrators
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }
            else if (User.IsInRole("Division Manager"))
            {
                meetingsQuery = meetingsQuery
                    .Where(m => m.MeetingAttendees == MeetingAttendees.Division_Managers
                             || m.MeetingAttendees == MeetingAttendees.Everyone);
            }

            var meetings = await meetingsQuery.ToListAsync();

            return View(meetings);
        }


        [Authorize(Roles =("Sport Administrator"))]
        public IActionResult Create()
        {
            ViewData["MeetingsAttendes"] = Enum.GetValues(typeof(MeetingAttendees))
                     .Cast<MeetingAttendees>()
                     .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            return View();
        }


        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MeetingViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;

                var userId = user.Id;

                var newMeeting = new Meeting
                {
                    MeetingTitle = viewModel.MeetingTitle,
                    MeetingDescription = viewModel.MeetingDescription,
                    AdditionalComments = viewModel.AdditionalComments,
                    Venue = viewModel.Venue,
                    MeetingDate = viewModel.MeetingDate,
                    MeetingTime = viewModel.MeetingTime,
                    CreatedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = userId,
                    ModifiedDateTime = DateTime.Now,
                    MeetingAttendees = viewModel.MeetingAttendes,
                    DivisionId = divisionId
                };

                newMeeting.MeetingStatus = MeetingStatus.Upcoming;

                _context.Add(newMeeting);
                await _context.SaveChangesAsync();

                var savedMeeting = await _context.Meeting
                    .Where(s => s.Equals(newMeeting))
                    .FirstOrDefaultAsync();

                TempData["Message"] = $"You have successfully scheduled a meeting that will take place at {viewModel.Venue} on {viewModel.MeetingDate} at {viewModel.MeetingTime}. You invited {viewModel.MeetingAttendes} to join the meeting.";

                await _activityLogger.Log($"Scheduled a meeting that will take place as follows; Date: {savedMeeting.MeetingDate:ddd, dd MMM yyyy}, Time: {savedMeeting.MeetingTime:HH:mm}", user.Id);

                await _requestLogService.LogSuceededRequest("Meeting scheduled successfully.", StatusCodes.Status200OK);

                var roleMapping = new Dictionary<MeetingAttendees, string>
        {
            { MeetingAttendees.Club_Administrators, "Club Administrator" },
            { MeetingAttendees.Club_Managers, "Club Manager" },
            { MeetingAttendees.Players, "Player" },
            { MeetingAttendees.Sport_Administrators, "Sport Administrator" },
            { MeetingAttendees.News_Updaters, "News Updator" },
            { MeetingAttendees.Sport_Coordinators, "Sport Coordinator" },
            { MeetingAttendees.Officials, "Official" },
            { MeetingAttendees.News_Administrators, "News Administrator" },
            { MeetingAttendees.Fans_Administrators, "Fans Administrator" },
            { MeetingAttendees.Personnel_Administrators, "Personnel Administrator" },
            { MeetingAttendees.Division_Managers, "Sport Manager" }
        };

                var subject = "Meeting Scheduled Notification";
                var emailBodyTemplate = $@"
            Dear {{0}},<br/><br/>
            A new meeting has been scheduled with the following details:<br/><br/>
            Title: {viewModel.MeetingTitle}<br/>
            Description: {viewModel.MeetingDescription}<br/>
            Venue: {viewModel.Venue}<br/>
            Date: {viewModel.MeetingDate:ddd, dd MMM yyyy}<br/>
            Time: {viewModel.MeetingTime:HH:mm}<br/>
            Additional Comments: {viewModel.AdditionalComments}<br/><br/>
            Please make a note of this meeting in your calendar.<br/><br/>
            If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
            Regards,<br/>
            Diski360 Management
                ";

                var usersInRoles = new List<UserBaseModel>();

                if (viewModel.MeetingAttendes == MeetingAttendees.Everyone)
                {
                    var allRoles = roleMapping.Values;
                    foreach (var roleName in allRoles)
                    {
                        usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                    }
                }
                else
                {
                    if (roleMapping.TryGetValue(viewModel.MeetingAttendes, out var roleName))
                    {
                        usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                    }
                }

                var uniqueUsers = usersInRoles.Distinct().ToList();

                foreach (var userInRole in uniqueUsers)
                {
                    var personalizedEmailBody = string.Format(emailBodyTemplate, $"{userInRole.FirstName} {userInRole.LastName}");
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(userInRole.Email, subject, personalizedEmailBody, "Diski 360"));
                }

                return RedirectToAction(nameof(Meetings));
            }

            ViewData["MeetingsAttendes"] = Enum.GetValues(typeof(MeetingAttendees))
                    .Cast<MeetingAttendees>()
                    .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() });

            await _requestLogService.LogFailedRequest("Failed to schedule a meeting.", StatusCodes.Status500InternalServerError);


            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdateMeeting(int? meetingId)
        {
            if (meetingId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var existingMeeting = await _context.Meeting
                .Where(e => e.MeetingId == meetingId &&
                e.DivisionId == divisionId)
                 
                .FirstOrDefaultAsync();

            var viewModel = new UpdateMeetingViewModel
            {
                MeetingTitle = existingMeeting.MeetingTitle,
                MeetingDescription = existingMeeting.MeetingDescription,
                AdditionalComments = existingMeeting.AdditionalComments,
                MeetingDate = existingMeeting.MeetingDate,
                MeetingTime = existingMeeting.MeetingTime,
                Venue = existingMeeting.Venue,
                MeetingAttendees = existingMeeting.MeetingAttendees
            };

            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        public async Task<IActionResult> UpdateMeeting(int? meetingId, UpdateMeetingViewModel viewModel)
        {
            if (meetingId != viewModel.MeetingId)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            if (ValidateUpdatedProperties(viewModel))
            {
                var existingMeeting = await _context.Meeting
                    .Where(e => e.MeetingId == meetingId &&
                    e.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (existingMeeting == null)
                {
                    return NotFound();
                }

                existingMeeting.MeetingTitle = viewModel.MeetingTitle;
                existingMeeting.MeetingDescription = viewModel.MeetingDescription;
                existingMeeting.Venue = viewModel.Venue;
                existingMeeting.MeetingDate = viewModel.MeetingDate;
                existingMeeting.MeetingTime = viewModel.MeetingTime;
                existingMeeting.AdditionalComments = viewModel.AdditionalComments;
                existingMeeting.ModifiedById = user.Id;
                existingMeeting.ModifiedDateTime = DateTime.Now; 

                _context.Update(existingMeeting);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"You have successfully updated a meeting with title {existingMeeting.MeetingTitle}";
                await _activityLogger.Log($"Updated a meeting with title {existingMeeting.MeetingTitle}", user.Id);
                await _requestLogService.LogSuceededRequest("Meeting updated successfully.", StatusCodes.Status200OK);

                var roleMapping = new Dictionary<MeetingAttendees, string>
        {
            { MeetingAttendees.Club_Administrators, "Club Administrator" },
            { MeetingAttendees.Club_Managers, "Club Manager" },
            { MeetingAttendees.Players, "Player" },
            { MeetingAttendees.Sport_Administrators, "Sport Administrator" },
            { MeetingAttendees.News_Updaters, "News Updator" },
            { MeetingAttendees.Sport_Coordinators, "Sport Coordinator" },
            { MeetingAttendees.Officials, "Official" },
            { MeetingAttendees.News_Administrators, "News Administrator" },
            { MeetingAttendees.Fans_Administrators, "Fans Administrator" },
            { MeetingAttendees.Personnel_Administrators, "Personnel Administrator" },
            { MeetingAttendees.Division_Managers, "Sport Manager" }
        };

                var subject = "Meeting Updated Notification";
                var emailBodyTemplate = $@"
            Dear {{0}},<br/><br/>
            A meeting has been updated with the following details:<br/><br/>
            Title: {viewModel.MeetingTitle}<br/>
            Description: {viewModel.MeetingDescription}<br/>
            Venue: {viewModel.Venue}<br/>
            Date: {viewModel.MeetingDate:ddd, dd MMM yyyy}<br/>
            Time: {viewModel.MeetingTime:HH:mm}<br/>
            Additional Comments: {viewModel.AdditionalComments}<br/><br/>
            Please make a note of this update in your calendar.<br/><br/>
            If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
            Regards,<br/>
              Diski360 Management
                ";

                var usersInRoles = new List<UserBaseModel>();

                if (viewModel.MeetingAttendees == MeetingAttendees.Everyone)
                {
                    foreach (var roleName in roleMapping.Values)
                    {
                        usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                    }
                }
                else
                {
                    if (roleMapping.TryGetValue(viewModel.MeetingAttendees, out var roleName))
                    {
                        usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                    }
                }

                var uniqueUsers = usersInRoles.Distinct().ToList();

                foreach (var userInRole in uniqueUsers)
                {
                    var personalizedEmailBody = string.Format(emailBodyTemplate, $"{userInRole.FirstName} {userInRole.LastName}");
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(userInRole.Email, subject, personalizedEmailBody, "Diski 360"));
                }

                return RedirectToAction(nameof(Meetings));
            }

            await _requestLogService.LogFailedRequest("Failed to update meeting.", StatusCodes.Status500InternalServerError);

            return View(viewModel);
        }


        private bool ValidateUpdatedProperties(UpdateMeetingViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.MeetingDate, new ValidationContext(viewModel, null, null) { MemberName = "MeetingDate" }, validationResults);
            Validator.TryValidateProperty(viewModel.MeetingTitle, new ValidationContext(viewModel, null, null) { MemberName = "MeetingTitle" }, validationResults);
            Validator.TryValidateProperty(viewModel.MeetingTime, new ValidationContext(viewModel, null, null) { MemberName = "MeetingTime" }, validationResults);
            Validator.TryValidateProperty(viewModel.MeetingDescription, new ValidationContext(viewModel, null, null) { MemberName = "MeetingDescription" }, validationResults);
            Validator.TryValidateProperty(viewModel.AdditionalComments, new ValidationContext(viewModel, null, null) { MemberName = "AdditionalComments" }, validationResults);
            Validator.TryValidateProperty(viewModel.Venue, new ValidationContext(viewModel, null, null) { MemberName = "Venue" }, validationResults);
            return validationResults.Count == 0;
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpGet]
        public async Task<IActionResult> MeetingDetails(int? meetingId)
        {
            if(meetingId == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meeting
                .Where(m => m.MeetingId == meetingId)
                .FirstOrDefaultAsync();


            var viewModel = new MeetingDetailsViewModel
            {
                MeetingAttendees = meeting.MeetingAttendees,
                MeetingTitle = meeting.MeetingTitle,
                MeetingDescription = meeting.MeetingDescription,
                Venue = meeting.Venue,
                MeetingDate = meeting.MeetingDate,
                MeetingTime = meeting.MeetingTime,
                AdditionalComments = meeting.AdditionalComments,
                MeetingStatus = meeting.MeetingStatus,
            };


            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> PostponeMeeting(int? meetingId)
        {
            if (meetingId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var meeting = await _context.Meeting
                .Where(m => m.MeetingId == meetingId)
                .FirstOrDefaultAsync();

            if (meeting == null)
            {
                return NotFound();
            }

            meeting.MeetingStatus = MeetingStatus.Postponed;
            meeting.ModifiedById = user.Id;
            meeting.ModifiedDateTime = DateTime.Now; 

            _context.Update(meeting);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"You have successfully postponed a meeting with title {meeting.MeetingTitle}";
            await _activityLogger.Log($"Postponed a meeting with title {meeting.MeetingTitle}", user.Id);
            await _requestLogService.LogSuceededRequest("Meeting postponed successfully.", StatusCodes.Status200OK);

            var roleMapping = new Dictionary<MeetingAttendees, string>
            {
        { MeetingAttendees.Club_Administrators, "Club Administrator" },
        { MeetingAttendees.Club_Managers, "Club Manager" },
        { MeetingAttendees.Players, "Player" },
        { MeetingAttendees.Sport_Administrators, "Sport Administrator" },
        { MeetingAttendees.News_Updaters, "News Updator" },
        { MeetingAttendees.Sport_Coordinators, "Sport Coordinator" },
        { MeetingAttendees.Officials, "Official" },
        { MeetingAttendees.News_Administrators, "News Administrator" },
        { MeetingAttendees.Fans_Administrators, "Fans Administrator" },
        { MeetingAttendees.Personnel_Administrators, "Personnel Administrator" },
        { MeetingAttendees.Division_Managers, "Sport Manager" }
           };

            var subject = "Meeting Postponed Notification";
            var emailBodyTemplate = $@"
        Dear {{0}},<br/><br/>
        The meeting with the title {meeting.MeetingTitle} has been postponed.<br/><br/>
        Please update your calendar accordingly.<br/><br/>
        If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
        Regards,<br/>
         Diski360 Management
            ";


            var usersInRoles = new List<UserBaseModel>();

            if (meeting.MeetingAttendees == MeetingAttendees.Everyone)
            {
                foreach (var roleName in roleMapping.Values)
                {
                    usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                }
            }
            else
            {
                if (roleMapping.TryGetValue(meeting.MeetingAttendees, out var roleName))
                {
                    usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                }
            }

            var uniqueUsers = usersInRoles.Distinct().ToList();

            foreach (var userInRole in uniqueUsers)
            {
                var personalizedEmailBody = string.Format(emailBodyTemplate, $"{userInRole.FirstName} {userInRole.LastName}");
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(userInRole.Email, subject, personalizedEmailBody, "Diski 360"));
            }

            return RedirectToAction(nameof(Meetings));
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> CancelMeeting(int? meetingId)
        {
            if (meetingId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var meeting = await _context.Meeting
                .Where(m => m.MeetingId == meetingId)
                .FirstOrDefaultAsync();

            if (meeting == null)
            {
                return NotFound();
            }

            meeting.MeetingStatus = MeetingStatus.Cancelled;
            meeting.ModifiedById = user.Id;
            meeting.ModifiedDateTime = DateTime.Now; 

            _context.Update(meeting);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"You have cancelled a meeting with title {meeting.MeetingTitle}";
            await _activityLogger.Log($"Cancelled a meeting with title {meeting.MeetingTitle}", user.Id);
            await _requestLogService.LogSuceededRequest("Meeting cancelled successfully.", StatusCodes.Status200OK);

            var roleMapping = new Dictionary<MeetingAttendees, string>
           {
        { MeetingAttendees.Club_Administrators, "Club Administrator" },
        { MeetingAttendees.Club_Managers, "Club Manager" },
        { MeetingAttendees.Players, "Player" },
        { MeetingAttendees.Sport_Administrators, "Sport Administrator" },
        { MeetingAttendees.News_Updaters, "News Updator" },
        { MeetingAttendees.Sport_Coordinators, "Sport Coordinator" },
        { MeetingAttendees.Officials, "Official" },
        { MeetingAttendees.News_Administrators, "News Administrator" },
        { MeetingAttendees.Fans_Administrators, "Fans Administrator" },
        { MeetingAttendees.Personnel_Administrators, "Personnel Administrator" },
        { MeetingAttendees.Division_Managers, "Sport Manager" }
        };

            var subject = "Meeting Cancelled Notification";
            var emailBodyTemplate = $@"
        Dear {{0}},<br/><br/>
        The meeting with the title {meeting.MeetingTitle} has been cancelled.<br/><br/>
        Please disregard any previous notices about this meeting.<br/><br/>
        If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
        Regards,<br/>
        Diski360 Management
             ";

            var usersInRoles = new List<UserBaseModel>();

            if (meeting.MeetingAttendees == MeetingAttendees.Everyone)
            {
                foreach (var roleName in roleMapping.Values)
                {
                    usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                }
            }
            else
            {
                if (roleMapping.TryGetValue(meeting.MeetingAttendees, out var roleName))
                {
                    usersInRoles.AddRange(await _userManager.GetUsersInRoleAsync(roleName));
                }
            }

            var uniqueUsers = usersInRoles.Distinct().ToList();

            foreach (var userInRole in uniqueUsers)
            {
                var personalizedEmailBody = string.Format(emailBodyTemplate, $"{userInRole.FirstName} {userInRole.LastName}");
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(userInRole.Email, subject, personalizedEmailBody, "Diski 360"));
            }

            return RedirectToAction(nameof(Meetings));
        }

    }
}
