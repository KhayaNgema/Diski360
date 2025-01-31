using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;

namespace MyField.Controllers
{
    public class SubscriptionsController : Controller
    {
        public readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly DeviceInfoService _deviceInfoService;
        private readonly IActivityLogger _activityLogger;
        private readonly IPaymentService _paymentService;
        private readonly RequestLogService _requestLogService;

        public SubscriptionsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            DeviceInfoService deviceInfoService,
            IActivityLogger activityLogger,
            IPaymentService paymentService,
            RequestLogService requestLogService)

        {
            _context = context;
            _userManager = userManager;
            _deviceInfoService = deviceInfoService;
            _activityLogger = activityLogger;
            _paymentService = paymentService;
            _requestLogService = requestLogService;
        }

        public async Task<IActionResult> MySubscriptionHistory()
        {
            var user = await _userManager.GetUserAsync(User);

            var mySubscriptionHistory = await _context.SubscriptionHistories
                .Where(us => us.UserId == user.Id)
                .Include(us => us.Subscription)
                .OrderByDescending(us => us.SubscribedDate)
                .ToListAsync();

            return View(mySubscriptionHistory);
        }

        public async Task<IActionResult> MyClubSubscriptionHistory()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return RedirectToAction("Error", "Home");
            }

            var myClubSubscriptions = await _context.SubscriptionHistories
                .Where(us => us.ClubId == clubAdministrator.ClubId)
                .ToListAsync();

            var club = await _context.Club
                .Where(c => c.ClubId == clubAdministrator.ClubId)
                .FirstOrDefaultAsync();

            ViewBag.ClubName = club?.ClubName;

            return View(myClubSubscriptions);
        }

        public async Task<IActionResult> UserSubscriptions()
        {
            var userSubscriptions = await _context.Subscriptions
                .Where(us => us.Club == null && 
                       us.SystemUser != null)
                .Include(s => s.SystemUser)
                .ToListAsync();

            return View(userSubscriptions);
        }

        public async Task<IActionResult> ClubSubscriptions()
        {
            var clubSubscriptions = await _context.Subscriptions
                .Where(us => us.Club != null &&
                       us.SystemUser == null)
                .Include(s => s.Club)
                .ThenInclude(us => us.Division)
                .ToListAsync();

            return View(clubSubscriptions);
        }

        public async Task<IActionResult> Unsubscribe()
        {
            return View();
        }

        public async Task<IActionResult> SuccessfullySubscribed(SubscriptionPlan subscriptionPlan)
        {
            var user = await _userManager.GetUserAsync(User);

            var loggedInUser = await _context.UserBaseModel
                .Where(liu => liu.Id == user.Id)
                .FirstOrDefaultAsync();

            ViewBag.FullNames = $"{loggedInUser.FirstName} {loggedInUser.LastName}";

            ViewBag.SubscriptionPlan = subscriptionPlan;

            return View();
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ClubSubscribe()
        {

            var user = await _userManager.GetUserAsync(User);

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            var subscription = await _context.Subscriptions
                .Where(s => s.ClubId == clubId)
                .FirstOrDefaultAsync();

            if (subscription != null)
            {
                ViewBag.SubscriptionPlan = subscription.SubscriptionPlan;
            }
            else
            {
                ViewBag.SubscriptionPlan = null;
            }

            return View();
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> UserSubscribe()
        {

            var user = await _userManager.GetUserAsync(User);

            var subscription = await _context.Subscriptions
                .Where(s => s.UserId == user.Id)
                .FirstOrDefaultAsync(); 

            if (subscription != null)
            {
                ViewBag.SubscriptionPlan = subscription.SubscriptionPlan; 
            }
            else
            {
                ViewBag.SubscriptionPlan = null;
            }

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubscribeBasic()
        {
            var user = await _userManager.GetUserAsync(User);

                var existingBasicSubscription = await _context.Subscriptions
                    .Where(ebs => ebs.UserId == user.Id)
                    .FirstOrDefaultAsync();

            existingBasicSubscription.SubscriptionPlan = SubscriptionPlan.Basic;
            existingBasicSubscription.SubscriptionStatus = SubscriptionStatus.Active;
            _context.Update(existingBasicSubscription);
            _context.SaveChangesAsync();

            return RedirectToAction(nameof(SuccessfullySubscribed), new { subscriptionPlan = SubscriptionPlan.Basic });

        }


        [HttpPost]
        public async Task<IActionResult> SubscribePremium()
        {
            var user = await _userManager.GetUserAsync(User);

            var deviceInfo = await _deviceInfoService.GetDeviceInfo();

            _context.Add(deviceInfo);
            await _context.SaveChangesAsync();

            try
            {
                var newPayment = new Payment
                {
                    ReferenceNumber = GeneratePremiumPaymentReferenceNumber(),
                    PaymentMethod = PaymentMethod.Credit_Card,
                    AmountPaid = 19.99m,
                    PaymentDate = DateTime.Now,
                    PaymentMadeById = user.Id,
                    Status = PaymentPaymentStatus.Unsuccessful,
                    DeviceInfoId = deviceInfo.DeviceInfoId,
                    DivisionId = null
                };

                _context.Add(newPayment);
                await _context.SaveChangesAsync();

                int paymentId = newPayment.PaymentId;
                decimal totalPrice = newPayment.AmountPaid;

                var returnUrl = Url.Action("PayFastReturnPremium", "Subscriptions",
                    new { paymentId, totalPrice }, Request.Scheme);

                var cancelUrl = "https://newcafeteriabykhaya.azurewebsites.net";

                string subscriptionName = "Premium Subscription";
                int subscriptionFrequency = 3;
                int subscriptionCycles = 12;

                string paymentUrl = GeneratePayFastSubscriptionPaymentUrl(
                    paymentId,
                    totalPrice,
                    returnUrl,
                    cancelUrl,
                    subscriptionName,
                    subscriptionFrequency,
                    subscriptionCycles);

                await _activityLogger.Log($"Initiated payment for premium subscription.", user.Id);

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the payment: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Failed to process payment.",
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message ?? "No inner exception available.",
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }


        [Authorize]
        public async Task<IActionResult> PayFastReturnPremium(int paymentId, decimal totalPrice)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not authenticated.");
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
                if (payment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment with PaymentId: {paymentId} not found.");
                    return Json(new { success = false, message = "Payment not found." });
                }

                decimal roundedAmountPaid = Math.Round(payment.AmountPaid, 2);
                decimal roundedTotalPrice = Math.Round(totalPrice, 2);

                if (Math.Abs(roundedAmountPaid - roundedTotalPrice) > 0.01m)
                {
                    System.Diagnostics.Debug.WriteLine($"Amount mismatch: Payment AmountPaid = {roundedAmountPaid}, totalPrice = {roundedTotalPrice}");
                    return Json(new { success = false, message = $"Invalid payment amount. AmountPaid: {roundedAmountPaid}, totalPrice: {roundedTotalPrice}" });
                }

                if (!_paymentService.ValidatePayment(payment))
                {
                    return Json(new { success = false, message = "Payment validation failed." });
                }

                var existingSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(es => es.UserId == user.Id);

                if (existingSubscription == null)
                {
                    return Json(new { success = false, message = "No active subscription found for the user." });
                }

                var activeHistory = await _context.SubscriptionHistories
                           .FirstOrDefaultAsync(sh => sh.UserId == user.Id && sh.Status == SubscriptionStatus.Active &&
                           sh.Plan == SubscriptionPlan.Premium);
                
                if (activeHistory != null)
                {
                    activeHistory.Status = SubscriptionStatus.Expired;
                    _context.Update(activeHistory);
                }

                existingSubscription.ExpirationDate = DateTime.Now.AddMonths(1);
                existingSubscription.SubscriptionStatus = SubscriptionStatus.Active;
                existingSubscription.SubscriptionPlan = SubscriptionPlan.Premium;
                existingSubscription.Amount = payment.AmountPaid;
                _context.Update(existingSubscription);

                payment.Status = PaymentPaymentStatus.Successful;
                _context.Update(payment);

                var newSubscriptionHistory = new SubscriptionHistory
                {
                    UserId = user.Id,
                    ExpiryDate = existingSubscription.ExpirationDate,
                    Plan = SubscriptionPlan.Premium,
                    Status = SubscriptionStatus.Active,
                    SubscribedDate = DateTime.UtcNow,
                    AmountPaid = payment.AmountPaid,
                    SubscriptionId = existingSubscription.SubscriptionId
                };

                _context.Add(newSubscriptionHistory);

                var existingCompetition = await _context.Competition
                    .Where(ec => ec.CompetitionStatus == CompetitionStatus.Current)
                    .FirstOrDefaultAsync();

                if (existingCompetition == null)
                {
                    var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    var newCompetition = new Competition
                    {
                        CompetitionStatus = CompetitionStatus.Current,
                        Month = currentMonthStart,
                        NumberOfParticipants = 0,
                    };

                    _context.Competition.Add(newCompetition);
                    await _context.SaveChangesAsync();

                    existingCompetition = await _context.Competition
                        .FirstOrDefaultAsync(c => c.CompetitionStatus == CompetitionStatus.Current && c.Month == currentMonthStart);
                }

                if (existingCompetition == null)
                {
                    throw new InvalidOperationException("Failed to create or fetch the current competition.");
                }

                var existingParticipant = await _context.CompetitionParticipants
                    .Where(ep => ep.UserId == user.Id && ep.CompetitionId == existingCompetition.CompetitionId)
                    .FirstOrDefaultAsync();

                if (existingParticipant == null)
                {
                    var newParticipant = new CompetitionParticipants
                    {
                        UserId = user.Id,
                        Points = 0,
                        CompetitionId = existingCompetition.CompetitionId
                    };

                    _context.CompetitionParticipants.Add(newParticipant);
                    existingCompetition.NumberOfParticipants++;

                    _context.Competition.Update(existingCompetition);
                    await _context.SaveChangesAsync();
                }



                await _activityLogger.Log($"Successfully renewed subscription to Premium.", user.Id);

                return RedirectToAction(nameof(SuccessfullySubscribed), new { subscriptionPlan = SubscriptionPlan.Premium });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to process payment: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }




        [HttpPost]
        public async Task<IActionResult> SubscribeClubPremium()
        {
            var user = await _userManager.GetUserAsync(User);
            var deviceInfo = await _deviceInfoService.GetDeviceInfo();

            _context.Add(deviceInfo);
            await _context.SaveChangesAsync();

            try
            {
                var newPayment = new Payment
                {
                    ReferenceNumber = GenerateClubPremiumPaymentReferenceNumber(),
                    PaymentMethod = PaymentMethod.Credit_Card,
                    AmountPaid = 49.99m,
                    PaymentDate = DateTime.Now,
                    PaymentMadeById = user.Id,
                    Status = PaymentPaymentStatus.Unsuccessful,
                    DeviceInfoId = deviceInfo.DeviceInfoId,
                    DivisionId = null
                };

                _context.Add(newPayment);
                await _context.SaveChangesAsync();

                int paymentId = newPayment.PaymentId;
                decimal totalPrice = newPayment.AmountPaid;

                var returnUrl = Url.Action("PayFastReturnClubPremium", "Subscriptions",
                    new { paymentId, totalPrice }, Request.Scheme);
                var cancelUrl = "https://newcafeteriabykhaya.azurewebsites.net";

                string subscriptionName = "Club Premium Subscription";
                int subscriptionFrequency = 3;
                int subscriptionCycles = 12;

                string paymentUrl = GeneratePayFastSubscriptionPaymentUrl(
                    paymentId,
                    totalPrice,
                    returnUrl,
                    cancelUrl,
                    subscriptionName,
                    subscriptionFrequency,
                    subscriptionCycles);

                await _activityLogger.Log($"Initiated payment for premium subscription.", user.Id);

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the payment: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Failed to process payment.",
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message ?? "No inner exception available.",
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }




        [Authorize]
        public async Task<IActionResult> PayFastReturnClubPremium(int paymentId, decimal totalPrice)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                var clubId = (user as ClubAdministrator)?.ClubId ??
                             (user as ClubManager)?.ClubId ??
                             (user as Player)?.ClubId;

                var existingSubscription = await _context.Subscriptions
                    .Where(es => es.ClubId == clubId)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment with PaymentId: {paymentId} not found.");
                    return Json(new { success = false, message = "Payment not found." });
                }

                decimal roundedAmountPaid = Math.Round(payment.AmountPaid, 2);
                decimal roundedTotalPrice = Math.Round(totalPrice, 2);

                if (Math.Abs(roundedAmountPaid - roundedTotalPrice) > 0.01m)
                {
                    System.Diagnostics.Debug.WriteLine($"Amount mismatch: Payment AmountPaid = {roundedAmountPaid}, totalPrice = {roundedTotalPrice}");
                    return Json(new { success = false, message = $"Invalid payment amount. AmountPaid: {roundedAmountPaid}, totalPrice: {roundedTotalPrice}" });
                }

                if (!_paymentService.ValidatePayment(payment))
                {
                    return Json(new { success = false, message = "Payment validation failed." });
                }

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not authenticated.");
                    return Json(new { success = false, message = "User not authenticated." });
                }

                existingSubscription.SubscriptionPlan = SubscriptionPlan.Club_Premium;
                existingSubscription.ExpirationDate = DateTime.Now.AddMonths(1);
                payment.Status = PaymentPaymentStatus.Successful;
                existingSubscription.SubscriptionStatus = SubscriptionStatus.Active;
                existingSubscription.Amount = payment.AmountPaid;

                _context.Update(payment);
                _context.Update(existingSubscription);
                await _context.SaveChangesAsync();

                var previousSubscriptionHistory = await _context.SubscriptionHistories
                    .Where(psh => psh.ClubId == existingSubscription.ClubId && 
                     psh.Status == SubscriptionStatus.Active &&
                     psh.Plan == SubscriptionPlan.Club_Premium)
                    .FirstOrDefaultAsync();

                if (previousSubscriptionHistory != null) 
                {
                    previousSubscriptionHistory.Status = SubscriptionStatus.Expired;

                    _context.Update(previousSubscriptionHistory);
                    await _context.SaveChangesAsync();
                }

                var newSubscriptionHistory = new SubscriptionHistory
                {
                    ClubId = existingSubscription.ClubId,
                    ExpiryDate = existingSubscription.ExpirationDate,
                    Plan = existingSubscription.SubscriptionPlan,
                    Status = existingSubscription.SubscriptionStatus,
                    SubscribedDate = DateTime.Now,
                    AmountPaid = existingSubscription.Amount,
                    SubscriptionId = existingSubscription.SubscriptionId
                };

                _context.Add(newSubscriptionHistory);


                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Successfully subscribed to club premium content.", user.Id);

                return RedirectToAction(nameof(SuccessfullySubscribed), new { subscriptionPlan = SubscriptionPlan.Club_Premium });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to process payment: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }

        private string GeneratePayFastSubscriptionPaymentUrl(
      int paymentId,
      decimal amount,
      string returnUrl,
      string cancelUrl,
      string subscriptionName,
      int subscriptionFrequency,
      int subscriptionCycles)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Invalid amount provided.", nameof(amount));
            }

            var validFrequencies = new[] { 1, 2, 3, 4 };

            if (!validFrequencies.Contains(subscriptionFrequency))
            {
                throw new ArgumentException("Invalid subscription frequency provided.", nameof(subscriptionFrequency));
            }

            if (subscriptionCycles <= 0)
            {
                throw new ArgumentException("Invalid subscription cycles provided.", nameof(subscriptionCycles));
            }

            var validSubscriptionItems = new[]
            {
 "Club Premium Subscription",
 "Premium Subscription"
};

            if (!validSubscriptionItems.Contains(subscriptionName))
            {
                throw new ArgumentException("Invalid subscription name provided.", nameof(subscriptionName));
            }

            const string MerchantId = "10033052";
            const string MerchantKey = "708c7udni72oo";

            string amountString = amount.ToString("F2", CultureInfo.InvariantCulture);

            var paymentUrl = $"https://sandbox.payfast.co.za/eng/process?" +
                             $"merchant_id={MerchantId}&" +
                             $"merchant_key={MerchantKey}&" +
                             $"return_url={UrlEncode(returnUrl)}&" +
                             $"cancel_url={UrlEncode(cancelUrl)}&" +
                             $"amount={amountString}&" +
                             $"item_name={UrlEncode(subscriptionName)}&" +
                             $"payment_id={paymentId}&" +
                             $"subscription_type=1&" +
                             $"frequency={subscriptionFrequency}&" +
                             $"cycles={subscriptionCycles}";

            return paymentUrl;
        }


        /* private string GeneratePayFastSubscriptionPaymentUrl(
       int paymentId,
       decimal amount,
       string returnUrl,
       string cancelUrl,
       string subscriptionName,
       int subscriptionFrequency,
       int subscriptionCycles)
         {
             if (amount <= 0)
             {
                 throw new ArgumentException("Invalid amount provided.", nameof(amount));
             }

             var validFrequencies = new[] { 1, 2, 3, 4 };

             if (!validFrequencies.Contains(subscriptionFrequency))
             {
                 throw new ArgumentException("Invalid subscription frequency provided.", nameof(subscriptionFrequency));
             }

             if (subscriptionCycles <= 0)
             {
                 throw new ArgumentException("Invalid subscription cycles provided.", nameof(subscriptionCycles));
             }

             var validSubscriptionItems = new[]
             {
         "Club Premium Subscription",
         "Premium Subscription"
     };

             if (!validSubscriptionItems.Contains(subscriptionName))
             {
                 throw new ArgumentException("Invalid subscription name provided.", nameof(subscriptionName));
             }

             const string MerchantId = "21098051";
             const string MerchantKey = "8oqhl4g4jjlft";

             string amountString = amount.ToString("F2", CultureInfo.InvariantCulture);

             var paymentUrl = $"https://www.payfast.co.za/eng/process?" +
                              $"merchant_id={MerchantId}&" +
                              $"merchant_key={MerchantKey}&" +
                              $"return_url={UrlEncode(returnUrl)}&" +
                              $"cancel_url={UrlEncode(cancelUrl)}&" +
                              $"amount={amountString}&" +
                              $"item_name={UrlEncode(subscriptionName)}&" +
                              $"payment_id={paymentId}&" +
                              $"subscription_type=1&" +
                              $"frequency={subscriptionFrequency}&" +
                              $"cycles={subscriptionCycles}";

             return paymentUrl;
         }*/

        // Helper method to URL encode strings
        private string UrlEncode(string input)
        {
            return WebUtility.UrlEncode(input);
        }

        private string GeneratePremiumPaymentReferenceNumber()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string numbers = "0123456789";
            const string premiumLetters = "PS";

            var random = new Random();
            var randomNumbers = new string(Enumerable.Repeat(numbers, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var premiumLets = premiumLetters.ToString();

            return $"{year}{month}{day}{randomNumbers}{premiumLets}";
        }

        private string GenerateClubPremiumPaymentReferenceNumber()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string numbers = "0123456789";
            const string premiumLetters = "PC";

            var random = new Random();
            var randomNumbers = new string(Enumerable.Repeat(numbers, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var premiumLets = premiumLetters.ToString();

            return $"{year}{month}{day}{randomNumbers}{premiumLets}";
        }


        [HttpPost]
        [Route("api/payfast/webhook")]
        public async Task<IActionResult> PayFastWebhook([FromBody] PayFastWebhookPayload payload)
        {
            if (!ValidatePayload(payload))
            {
                return BadRequest("Invalid payload");
            }

            try
            {
                if (payload.Status == "completed" || payload.Status == "successful")
                {
                    var subscriptionPlan = GetSubscriptionPlanFromItemName(payload.ItemName);

                    var subscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.UserId == payload.UserId);

                    if (subscription != null)
                    {
                        subscription.SubscriptionPlan = subscriptionPlan;
                        subscription.SubscriptionStatus = SubscriptionStatus.Active;
                        subscription.Amount = payload.AmountPaid; 
                                                                  
                        subscription.ExpirationDate = DateTime.Now.AddMonths(1);
                        _context.Update(subscription);
                    }
                    else
                    {
                        subscription = new Subscription
                        {
                            UserId = payload.UserId,
                            SubscriptionPlan = subscriptionPlan,
                            SubscriptionStatus = SubscriptionStatus.Active,
                            Amount = payload.AmountPaid,
                            ExpirationDate = DateTime.Now.AddMonths(1)
                        };
                        await _context.Subscriptions.AddAsync(subscription);
                    }

                    await _context.SaveChangesAsync();
                }
                else if (payload.Status == "failed" || payload.Status == "refunded")
                {
                    var subscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.UserId == payload.UserId);

                    if (subscription != null)
                    {
                        subscription.SubscriptionStatus = SubscriptionStatus.Cancelled;
                        _context.Update(subscription);
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private bool ValidatePayload(PayFastWebhookPayload payload)
        {
            return !string.IsNullOrEmpty(payload.PaymentId) &&
                   !string.IsNullOrEmpty(payload.UserId) &&
                   payload.AmountPaid > 0;
        }

        private SubscriptionPlan GetSubscriptionPlanFromItemName(string itemName)
        {
            return itemName switch
            {
                "Basic Subscription" => SubscriptionPlan.Basic,
                "Premium Subscription" => SubscriptionPlan.Premium,
                "Club Premium Subscription" => SubscriptionPlan.Club_Premium,
                _ => SubscriptionPlan.Basic 
            };
        }

    }
}
