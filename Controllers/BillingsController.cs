
using MyField.Models;
using MyField.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Identity;
using MyField.Data;
using MyField.Services;
using Microsoft.EntityFrameworkCore;
using MyField.Interfaces;
using System.Web;
using SelectPdf;
using System.Numerics;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;
using Hangfire;

namespace MyField.Controllers
{
    public class BillingsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly DeviceInfoService _deviceInfoService;
        private readonly PdfService _pdfService;
        private readonly IViewRenderService _viewRenderService;
        private readonly EmailService _emailService;
        private readonly IActivityLogger _activityLogger;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;


        public BillingsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            IPaymentService paymentService,
            DeviceInfoService deviceInfoService,
            PdfService pdfService,
            IViewRenderService viewRenderService,
            EmailService emailService,
            IActivityLogger activityLogger,
            IEncryptionService encryptionService,
            RequestLogService requestLogService)
        { 
            _emailService = emailService;
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
            _deviceInfoService = deviceInfoService;
            _pdfService = pdfService;
            _viewRenderService = viewRenderService; 
            _activityLogger = activityLogger;  
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        [Authorize]
        public async Task<IActionResult> MyIndividualFineInvoicePreview(int invoiceId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var invoice = await _context.Invoices
                .Where(i => i.InvoiceId == invoiceId && i.CreatedById == user.Id)
                .Include(i => i.Payment)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Offender)
                .FirstOrDefaultAsync();

            ViewBag.FullNames = user.FirstName + " " + user.LastName;

            return PartialView("_MyIndividualFineInvoicePartial", invoice);
        }

        [Authorize]
        public async Task<IActionResult> MyPlayerInvoicePDfPreview (int invoiceId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            int? clubId = user switch
            {
                ClubAdministrator clubAdmin => clubAdmin.ClubId,
                ClubManager clubManager => clubManager.ClubId,
                Player player => player.ClubId,
                _ => null
            };

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubs = await _context.Club
                                      .FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            var invoice = await _context.Invoices
                .Where(i => i.Payment.ClubId == clubId && i.InvoiceId == invoiceId)
                .Include(i => i.Payment)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.Player)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.SellerClub)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.CustomerClub)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.League)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.Division)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Club)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Offender)
                .FirstOrDefaultAsync();

            ViewBag.ClubName = clubs?.ClubName;
            ViewBag.DivisionName = invoice.Transfer.Division.DivisionName;
            ViewBag.DivisionLogo = invoice.Transfer.Division.DivisionBadge;
            ViewBag.Season = invoice.Transfer.League.LeagueYears;

            var sellerClub = await _context.Club
                .Where(sc => sc.ClubId == invoice.Transfer.SellerClub.ClubId)
                .FirstOrDefaultAsync();

            var sellerClubAdmin = await _context.ClubAdministrator
                .Where(sca => sca.ClubId == sellerClub.ClubId)
                .FirstOrDefaultAsync();

            ViewBag.SellerClubAdminSignature = $"{sellerClubAdmin.FirstName} {sellerClubAdmin.LastName}";

            return PartialView("_MyPlayerTransferInvoicePartial", invoice);
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> MyClubFineInvoicePreview(int invoiceId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            int? clubId = user switch
            {
                ClubAdministrator clubAdmin => clubAdmin.ClubId,
                ClubManager clubManager => clubManager.ClubId,
                Player player => player.ClubId,
                _ => null
            };

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubs = await _context.Club
                .Where(mo => mo.ClubId == clubId)
                .Include(mo => mo.League)
                .FirstOrDefaultAsync();

            var invoice = await _context.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Include(i => i.Payment)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.Player)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.SellerClub)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.CustomerClub)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Club)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Offender)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Division)
                .FirstOrDefaultAsync();

            ViewBag.ClubName = clubs?.ClubName;
            ViewBag.DivisionName = invoice.Fine.Division.DivisionName;
            ViewBag.DivisionLogo = invoice.Fine.Division.DivisionBadge;
            ViewBag.Season = clubs.League.LeagueYears;

            var clubAdministrator = await _context.ClubAdministrator
                .Where(ca => ca.ClubId == clubs.ClubId)
                .FirstOrDefaultAsync();

            ViewBag.ClubAdmin = $"{clubAdministrator.FirstName} {clubAdministrator.LastName}";

            return PartialView("_MyClubFineInvoicePartial", invoice);
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IndividualFineInvoice()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var individualInvoices = await _context.Invoices
                .Where(i => i.Payment.PaymentMadeById == userId)    
                .Include(i => i.Payment)
                .ToArrayAsync();

            return View(individualInvoices);
        }


        [Authorize(Roles = ("Club Administrator"))]
        [HttpGet]
        public async Task<IActionResult> ClubInvoice()
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

            var clubs = await _context.Club
                                      .FirstOrDefaultAsync(mo => mo.ClubId == clubId);


            var invoice = await _context.Invoices
                .Where(i => i.Payment.ClubId == clubId || i.Payment.PaymentMadeById == user.Id)
                .Include(i => i.CreatedBy)
                .Include(i => i.Transfer)
                .ThenInclude(t => t.CreatedBy)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Club)
                .Include(i => i.Fine)
                .ThenInclude(t => t.Offender)
                .Include(i => i.Payment)
                .ThenInclude(t => t.PaymentMadeByClub)
                .Include(i => i.Payment)
                .ThenInclude(t => t.PaymentMadeBy)
                .ToListAsync();

            ViewBag.ClubName = clubs?.ClubName;

            return View(invoice);
        }


        [Authorize(Roles = "Division Manager, System Administrator")]
        [HttpGet]
        public async Task<IActionResult> Transactions()
        {
            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var paymentsQuery = _context.Payments
                .Where(p => p.DivisionId == divisionId)
                .Include(p => p.PaymentMadeBy);

            var payments = new List<Payment>();

            if (User.IsInRole("Division Manager"))
            {
                payments = await paymentsQuery
                    .Where(p => !(p.ReferenceNumber.EndsWith("PC") || p.ReferenceNumber.EndsWith("PS"))) // Exclude payments ending with "PC" or "PS"
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }
            else
            {
                payments = await paymentsQuery
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }

            return View(payments);
        }





        [Authorize(Roles = ("Division Manager, System Administrator"))]
        public async Task<IActionResult> PaymentDetails(int? paymentId)
        {
            if (paymentId == null || _context.Payments == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .Include(p => p.PaymentMadeBy)
                .Include(p => p.DeviceInfo)
                .FirstOrDefaultAsync();

            var viewModel = new PaymentViewModel
            {
                PaymentId = payment.PaymentId,
                AmountPaid = payment.AmountPaid,
                FirstName = payment.PaymentMadeBy.FirstName,
                LastName = payment.PaymentMadeBy.LastName,
                PaymentDate = payment.PaymentDate,
                PaymentStatus = payment.Status,
                DeviceName = $"{payment?.DeviceInfo?.DeviceName}, {payment?.DeviceInfo?.DeviceModel}, {payment?.DeviceInfo?.OSName}, {payment?.DeviceInfo?.OSVersion}",
                Browser = $"{payment?.DeviceInfo?.Browser}, {payment?.DeviceInfo?.BrowserVersion}",
                DeviceLocation = $"({payment?.DeviceInfo?.Longitude}, {payment?.DeviceInfo?.Latitude}), {payment?.DeviceInfo?.City}, {payment?.DeviceInfo?.Region},  {payment?.DeviceInfo?.Country} "
            };

            return View(viewModel);
        }


        [Authorize(Roles = ("Club Administrator"))]
        [HttpGet]
        public async Task<IActionResult> PayClubFIne(int fineId, int clubId)
        {
            var fine = await _context.Fines
                .Where(mo => mo.FineId == fineId)
                .FirstOrDefaultAsync();

            var club = await _context.Club
                .Where(mo => mo.ClubId == clubId)
                .FirstOrDefaultAsync();



            var viewModel = new PayClubFineViewModel
            {
                FineId = fineId,
                FineDetails = fine.FineDetails,
                FineAmount = fine.FineAmount,
                FineDuDate = fine.FineDuDate,
                ClubId = club.ClubId,
                ClubName = club.ClubName,
                ClubBadge = club.ClubBadge,
            };

            return View(viewModel);
        }

        [Authorize(Roles = ("Club Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayClubFIne(PayClubFineViewModel viewModel)
        {
            return View();
        }



        [Authorize(Roles = "Club Administrator")]
        [HttpGet]
        public async Task<IActionResult> PayPlayerTransfer(string transferId)
        {
            var decryptedTransferId = _encryptionService.DecryptToInt(transferId);

            var transfer = await _context.Transfer
                .Where(mo => mo.TransferId == decryptedTransferId)
                .Include(s => s.Player)
                .Include(s => s.SellerClub)
                .Include(s => s.CustomerClub)
                .Include(s => s.PlayerTransferMarket)
                .FirstOrDefaultAsync();

            if (transfer == null)
            {
                return NotFound();
            }

            var viewModel = new PayPlayerTransferViewModel
            {
                TransferId = decryptedTransferId,
                PlayerId = transfer.Player.Id,
                PlayerTransferMarketId = transfer.PlayerTransferMarketId,
                SellerClubId = transfer.SellerClubId,
                CustomerClubId = transfer.CustomerClubId,
                SellerClubName = transfer.SellerClub.ClubName,
                SellerClubBadge = transfer.SellerClub.ClubBadge,
                ProfilePicture = transfer.Player.ProfilePicture,
                BuyerClubName = transfer.CustomerClub.ClubName,
                BuyerClubBadge = transfer.CustomerClub.ClubBadge,
                FirstName = transfer.Player.FirstName,
                LastName = transfer.Player.LastName,
                PlayerAmount = transfer.Player.MarketValue,
                Position = transfer.Player.Position,
                JerseyNumber = transfer.Player.JerseyNumber,
                DateOfBirth = transfer.Player.DateOfBirth,
                PaymentStatus = transfer.paymentTransferStatus,
            };

            return View(viewModel);
        }


        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> PayPlayerTransfer(PayPlayerTransferViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }


                var deviceInfo = await _deviceInfoService.GetDeviceInfo();


                var divisionId = (user as ClubAdministrator)?.DivisionId;

                _context.Add(deviceInfo);
                await _context.SaveChangesAsync();

                var newPayment = new Payment
                {
                    ReferenceNumber = GenerateTransferPaymentReferenceNumber(),
                    PaymentMethod = PaymentMethod.Credit_Card,
                    AmountPaid = viewModel.PlayerAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMadeById = user.Id,
                    Status = PaymentPaymentStatus.Unsuccessful,
                    DeviceInfoId = deviceInfo.DeviceInfoId,
                    DivisionId = divisionId
                };

                _context.Add(newPayment);
                await _context.SaveChangesAsync();

                var player = await _context.Player.FindAsync(viewModel.PlayerId);

                var playerTransfer = await _context.Transfer
                    .Where(pt => pt.TransferId == viewModel.TransferId)
                    .FirstOrDefaultAsync();

                if (player == null)
                {
                    return Json(new { success = false, message = "Player not found." });
                }

                if (playerTransfer == null)
                {
                    return Json(new { success = false, message = "Player transfer not found." });
                }

                var encryptedTransferId = _encryptionService.Encrypt(viewModel.TransferId);

                int paymentId = newPayment.PaymentId;
                decimal totalPrice = viewModel.PlayerAmount;
                string transferId = encryptedTransferId;
                var returnUrl = Url.Action("PayFastReturn", "Billings", new { paymentId, encryptedTransferId, totalPrice }, Request.Scheme);
                returnUrl = HttpUtility.UrlEncode(returnUrl);
                var cancelUrl = "https://newcafeteriabykhaya.azurewebsites.net";

                string paymentUrl = await GeneratePayFastPaymentUrl(paymentId, totalPrice, encryptedTransferId, returnUrl, cancelUrl);

                await _activityLogger.Log($"Initiated payment for {player.FirstName} {player.LastName} transfer.", user.Id);

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

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<IActionResult> PayFastReturn(int paymentId, string encryptedTransferId, decimal totalPrice)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (payment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment with PaymentId: {paymentId} not found.");
                    return Json(new { success = false, message = "Payment not found." });
                }

               

                var decryptedTransferId = _encryptionService.DecryptToInt(encryptedTransferId);

                var playerTransfer = await _context.Transfer
                    .Where(mo => mo.TransferId == decryptedTransferId)
                    .Include(s => s.Player)
                    .Include(s => s.PlayerTransferMarket)
                    .Include(s => s.SellerClub)
                    .Include(s => s.CustomerClub)
                    .Include(s => s.Division)
                    .FirstOrDefaultAsync();

                var transferReport = await _context.TransfersReports
                    .Where(t => t.Season.IsCurrent && t.DivisionId == playerTransfer.DivisionId)
                    .FirstOrDefaultAsync();

                var sellerClubTransferReport = await _context.ClubTransferReports
                    .Where(c => c.ClubId == playerTransfer.SellerClub.ClubId &&
                    c.League.IsCurrent)
                    .Include(c => c.Club)
                    .FirstOrDefaultAsync();

                var buyerClubTransferReport = await _context.ClubTransferReports
                    .Where(c => c.ClubId == playerTransfer.CustomerClub.ClubId &&
                    c.League.IsCurrent)
                    .Include(c => c.Club)
                    .FirstOrDefaultAsync();

                if (playerTransfer == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Player transfer with TransferId: {decryptedTransferId} not found.");
                    return Json(new { success = false, message = $"Player transfer not found with TransferId: {decryptedTransferId}" });
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

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not authenticated.");
                    return Json(new { success = false, message = "User not authenticated." });
                }

                playerTransfer.Player.ClubId = playerTransfer.CustomerClubId;
                playerTransfer.Player.ModifiedBy = user.Id;
                playerTransfer.Player.ModifiedDateTime = DateTime.Now;
                playerTransfer.PlayerTransferMarket.SaleStatus = SaleStatus.Unavailable;
                playerTransfer.paymentTransferStatus = PaymentTransferStatus.Payment_Completed;
                playerTransfer.Status = TransferStatus.Completed;
                payment.Status = PaymentPaymentStatus.Successful;

                transferReport.PurchasedPlayersCount++;

                transferReport.TranferAmount += payment.AmountPaid;
                transferReport.AssociationCut = transferReport.TranferAmount * 0.335m;
                transferReport.ClubsCut = transferReport.TranferAmount * 0.665m;

                if (transferReport.TransferMarketCount == 0)
                {
                    return Json(new { success = false, message = "Transfer market count cannot be zero." });
                }


                buyerClubTransferReport.SuccessfulOutgoingTransfersCount++;
                sellerClubTransferReport.SuccessfulIncomingTransfersCount++;



                _context.Update(payment);
                _context.Update(playerTransfer);
                await _context.SaveChangesAsync();

                var newInvoice = new Invoice
                {
                    PaymentId = payment.PaymentId,
                    TransferId = playerTransfer.TransferId,
                    InvoiceTimeStamp = DateTime.Now,
                    CreatedById = user.Id,
                    InvoiceNumber = GenerateInvoiceNumber(paymentId),
                    IsEmailed = true,
                    Transfer = playerTransfer
                };

                payment.ClubId = playerTransfer.CustomerClubId; 

                _context.Update(payment);
                _context.Add(newInvoice);
                await _context.SaveChangesAsync();

                var viewData = new Invoice
                {
                    Transfer = playerTransfer,
                    Payment = payment,
                    InvoiceNumber = newInvoice.InvoiceNumber
                };

                string emailBody = $@"
            <p>Dear {playerTransfer.CustomerClub.ClubName} Management,</p>
            <p>We are pleased to inform you that the player transfer of {playerTransfer.Player.FirstName} {playerTransfer.Player.LastName} has been successfully completed.</p>
            <p>The proof of this transfer is available on your club's portal.</p>
            <p>Transfer Details:</p>
            <ul>
                <li><strong>Player:</strong> {playerTransfer.Player.FirstName} {playerTransfer.Player.LastName}</li>
                <li><strong>Transfer Amount:</strong> {payment.AmountPaid:C}</li>
                <li><strong>From Club:</strong> {playerTransfer.SellerClub.ClubName}</li>
                <li><strong>To Club:</strong> {playerTransfer.CustomerClub.ClubName}</li>
            </ul>
            <p>If you have any questions, feel free to contact us.</p>
            <p>Best regards,</p>
            <p>Diski 360 Team</p>
        ";


                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Proof of Player Transfer", emailBody, "Diski 360"));
                TempData["Message"] = $"You have successfully bought {playerTransfer.Player.FirstName} {playerTransfer.Player.LastName} at an amount of {playerTransfer.Player.MarketValue} from {playerTransfer.SellerClub.ClubName}";

                await _requestLogService.LogSuceededRequest("Successfully completed player transfer", StatusCodes.Status200OK);

                return RedirectToAction("MyTransfersTabs", "Transfers");
            }
            catch (Exception ex)
            {
                await _requestLogService.LogFailedRequest("Failed to complete player transfer", StatusCodes.Status500InternalServerError);

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

        [Authorize(Roles = ("Club Administrator"))]
        private async Task<string> GeneratePayFastPaymentUrl(int paymentId, decimal amount, string encryptedTransferId, string returnUrl, string cancelUrl)
        {
            var decryptedTransferId = _encryptionService.DecryptToInt(encryptedTransferId);

            var transfer = await _context.Transfer
                .Where(t => t.TransferId == decryptedTransferId)
                .Include(t=>t.Player)
                .FirstOrDefaultAsync();

            string merchantId = "10033052";
            string merchantKey = "708c7udni72oo";

            int amountInCents = (int)(amount * 100);
            string amountString = amount.ToString("0.00").Replace(',', '.');

            string playerName = $"{transfer.Player.FirstName} {transfer.Player.LastName} Transfer";
            string encodedPlayerName = Uri.EscapeDataString(playerName);
            string paymentUrl = $"https://sandbox.payfast.co.za/eng/process?merchant_id={merchantId}&merchant_key={merchantKey}&" +
                                $"return_url={returnUrl}&cancel_url={cancelUrl}&amount={amountInCents}&" +
                                $"item_name={encodedPlayerName}&payment_id={paymentId}&transfer_id={encryptedTransferId}&" +
                                $"amount={amountString}";


            return paymentUrl;
        }




        /* private string GeneratePayFastPaymentUrl(int paymentId, decimal amount, int transferId, string returnUrl, string cancelUrl)
         {
             var playerTransfer = _context.Transfer
                    .Where(mo => mo.TransferId == transferId)
                    .Include(s => s.Player)
                    .Include(s => s.PlayerTransferMarket)
                    .Include(s => s.SellerClub)
                    .Include(s => s.CustomerClub)
                    .FirstOrDefault();

             string merchantId = "21098051";
             string merchantKey = "8oqhl4g4jjlft";
             int amountInCents = (int)(amount * 100);
             string amountString = amount.ToString("0.00").Replace(',', '.');

             string paymentUrl = $"https://www.payfast.co.za/eng/process?merchant_id={merchantId}&merchant_key={merchantKey}&return_url={returnUrl}&cancel_url={cancelUrl}&amount={amountInCents}&item_name={playerTransfer.Player.FirstName} {playerTransfer.Player.LastName} transfer from {playerTransfer.SellerClub.ClubName} to {playerTransfer.CustomerClub.ClubName}&payment_id={paymentId}&transfer_id={transferId}&amount={amountString}";

             return paymentUrl;
         }*/



        /*Pay club and individual fines*/

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PayFines(string fineId)
        {
            var decryptedFineId = _encryptionService.DecryptToInt(fineId);


            var fine = await _context.Fines
                .Where(mo => mo.FineId == decryptedFineId)   
                .Include(f => f.Club)
                .Include(f => f.Offender)
                .FirstOrDefaultAsync();

            if (fine == null)
            {
                return NotFound();
            }

            var viewModel = new PayFineViewModel
            {
                FineId = decryptedFineId,    
                FineDetails = fine.FineDetails,
                FineAmount = fine.FineAmount,
                FineDueDate = fine.FineDuDate,
                PaymentStatus = fine.PaymentStatus,
            };

            return View(viewModel);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayFines(PayFineViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;

                var deviceInfo = await _deviceInfoService.GetDeviceInfo();

                _context.Add(deviceInfo);
                await _context.SaveChangesAsync();

                var newPayment = new Payment
                {
                    ReferenceNumber = GenerateFinePaymentReferenceNumber(),
                    PaymentMethod = PaymentMethod.Credit_Card,
                    AmountPaid = viewModel.FineAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMadeById = user.Id,
                    Status = PaymentPaymentStatus.Unsuccessful,
                    DeviceInfoId = deviceInfo.DeviceInfoId,
                    DivisionId = divisionId
                };

                _context.Add(newPayment);
                await _context.SaveChangesAsync();

                var fine = await _context.Fines.FindAsync(viewModel.FineId);

                if (fine == null)
                {
                    return Json(new { success = false, message = "Fine not found." });
                }

                var encryptedFineId = _encryptionService.Encrypt(viewModel.FineId);
                var encryptedPaymentId = _encryptionService.Encrypt(newPayment.PaymentId);

                int paymentId = newPayment.PaymentId;
                decimal totalPrice = viewModel.FineAmount;

                var returnUrl = Url.Action("PayFinePayFastReturn", "Billings", new { encryptedPaymentId, encryptedFineId, totalPrice }, Request.Scheme);
                returnUrl = HttpUtility.UrlEncode(returnUrl);
                var cancelUrl = "https://newcafeteriabykhaya.azurewebsites.net";

                string paymentUrl = GeneratePayFineFastPaymentUrl(encryptedPaymentId, totalPrice, encryptedFineId, returnUrl, cancelUrl);

                await _activityLogger.Log($"Initiated payment for {fine?.Offender?.FirstName} {fine?.Offender?.LastName} {fine?.Club?.ClubName} fine.", user.Id);

                return Redirect(paymentUrl);

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to redirect to payfast: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }

        [Authorize]
        public async Task<IActionResult> PayFinePayFastReturn(string encryptedPaymentId, string encryptedFineId, decimal totalPrice)
        {
            try
            {
                var decryptedFineId = _encryptionService.DecryptToInt(encryptedFineId);
                var decryptedPaymentId = _encryptionService.DecryptToInt(encryptedPaymentId);


                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == decryptedPaymentId);
                if (payment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Payment with PaymentId: {decryptedPaymentId} not found.");
                    return Json(new { success = false, message = "Payment not found." });
                }

                System.Diagnostics.Debug.WriteLine($"Payment found: {payment.PaymentId}, AmountPaid: {payment.AmountPaid}");

                var fine = await _context.Fines
                    .Where(mo => mo.FineId == decryptedFineId)
                    .Include(s => s.Club)
                    .Include(s => s.Offender)
                    .FirstOrDefaultAsync();

                if (fine == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Player transfer with TransferId: {decryptedFineId} not found.");
                    return Json(new { success = false, message = $"Player transfer not found with TransferId: {decryptedFineId}" });
                }

                System.Diagnostics.Debug.WriteLine($"Player transfer found: {fine.FineId}, PlayerId: {fine.OffenderId}");


                decimal roundedAmountPaid = Math.Round(payment.AmountPaid, 2);
                decimal roundedTotalPrice = Math.Round(totalPrice, 2);

                System.Diagnostics.Debug.WriteLine($"Original AmountPaid: {payment.AmountPaid}, roundedAmountPaid: {roundedAmountPaid}, Original totalPrice: {totalPrice}, roundedTotalPrice: {roundedTotalPrice}");

                if (Math.Abs(roundedAmountPaid - roundedTotalPrice) > 0.01m)
                {
                    System.Diagnostics.Debug.WriteLine($"Amount mismatch: Payment AmountPaid = {roundedAmountPaid}, totalPrice = {roundedTotalPrice}");

                    return Json(new
                    {
                        success = false,
                        message = $"Invalid payment amount. AmountPaid: {roundedAmountPaid}, totalPrice: {roundedTotalPrice}"
                    });
                }

                if (!_paymentService.ValidatePayment(payment))
                {
                    return Json(new { success = false, message = "Payment validation failed." });
                }

                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User not authenticated.");
                    return Json(new { success = false, message = "User not authenticated." });
                }
                var userId = user.Id;

                System.Diagnostics.Debug.WriteLine($"User found: {userId}");

                var personnelFinancialReport = await _context.PersonnelFinancialReports
                     .FirstOrDefaultAsync();

                if (fine.Offender != null)
                {

                    personnelFinancialReport.PaidPaymentFinesCount++;
                    personnelFinancialReport.TotalPaidAmount = personnelFinancialReport.TotalPaidAmount + payment.AmountPaid;
                }



                payment.PaymentMadeById = fine.OffenderId;
                payment.ClubId = fine.ClubId;   
                fine.PaymentStatus = PaymentStatus.Paid;
                payment.Status = PaymentPaymentStatus.Successful;
                fine.ModifiedById = userId;
                fine.ModifiedDateTime = DateTime.Now;
               

                _context.Update(fine);
                _context.Update(payment);
                await _context.SaveChangesAsync();

                var newInvoice = new Invoice
                {
                    PaymentId = payment.PaymentId,
                    FineId = fine.FineId,
                    InvoiceTimeStamp = DateTime.Now,
                    CreatedById = userId,
                };

                newInvoice.InvoiceNumber = GenerateInvoiceNumber(decryptedPaymentId);
                newInvoice.IsEmailed = true;

                _context.Add(newInvoice);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Player transfer and player updated successfully.");

                TempData["Message"] = $"You have successfully cleared your charges of {fine.FineAmount}. You will receive a confirmation email shortly with our clearence";

                

                if (fine.ClubId  != null)
                {
                    await _requestLogService.LogSuceededRequest("Successfully cleared club fines", StatusCodes.Status200OK);

                    return RedirectToAction("MyClubFines", "Fines");
                }
                else
                {
                    await _requestLogService.LogSuceededRequest("Successfully cleared individual fines", StatusCodes.Status200OK);

                    return RedirectToAction("MyIndividualFines", "Fines");
                }

            }
            catch (Exception ex)
            {

                await _requestLogService.LogFailedRequest("Failed to clear fines", StatusCodes.Status500InternalServerError);

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

        [Authorize]
        private string GeneratePayFineFastPaymentUrl(string encryptedPaymentId, decimal amount, string encryptedFineId, string returnUrl, string cancelUrl)
        {

            var decryptedFineId = _encryptionService.DecryptToInt(encryptedFineId);   

            var fine = _context.Fines
                .Where(mo => mo.FineId == decryptedFineId)
                .Include(f => f.Offender)
                .Include(f => f.Club)
                .FirstOrDefault();

            string merchantId = "10033052";
            string merchantKey = "708c7udni72oo";

            int amountInCents = (int)(amount * 100);
            string amountString = amount.ToString("0.00").Replace(',', '.');

            string fineDetails = fine?.FineDetails?.Replace("\r\n", "");

            string paymentUrl = $"https://sandbox.payfast.co.za/eng/process?merchant_id={merchantId}&merchant_key={merchantKey}&return_url={returnUrl}&cancel_url={cancelUrl}&amount={amountInCents}&item_name={fine?.Offender?.FirstName} {fine?.Offender?.LastName} {fine?.Club?.ClubName} fine charges for offence:{fineDetails}&payment_id={encryptedPaymentId}&fine_id={encryptedFineId}&amount={amountString}";

            return paymentUrl;
        }

        private string GenerateInvoiceNumber(int paymentId)
        {
            var payment = _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .FirstOrDefault();  

            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string numbers = "0123456789";

            var random = new Random();
            var randomNumbers = new string(Enumerable.Repeat(numbers, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{year}{month}{day}{randomNumbers}";
        }


        private string GenerateTransferPaymentReferenceNumber()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string numbers = "0123456789";
            const string transferLetters = "TF";

            var random = new Random();
            var randomNumbers = new string(Enumerable.Repeat(numbers, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var transLetters = transferLetters.ToString();  

            return $"{year}{month}{day}{randomNumbers}{transLetters}";
        }

        private string GenerateFinePaymentReferenceNumber()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string numbers = "0123456789";
            const string fineLetters = "FP";

            var random = new Random();
            var randomNumbers = new string(Enumerable.Repeat(numbers, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            var fineLets = fineLetters.ToString();  

            return $"{year}{month}{day}{randomNumbers}{fineLets}";
        }
    }
}
