
using MyField.Interfaces;
using MyField.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyField.Services
{
    public class PayFastPaymentService : IPaymentService
    {
        private const string MerchantKey = "8oqhl4g4jjlft";
        private const string Passphrase = "Theroyalintrnetcafe50";

        public bool ValidatePayment(Payment payment)
        {
            if (payment.PaymentMethod != PaymentMethod.Credit_Card)
            {
                return false;
            }

            if (payment.AmountPaid <= 0)
            {
                return false;
            }

            return true;
        }

        public bool ProcessPayment(Payment payment)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.BaseAddress = new Uri("https://api.payfast.co.za/");

                    httpClient.DefaultRequestHeaders.Add("Merchant-Key", MerchantKey);
                    httpClient.DefaultRequestHeaders.Add("Passphrase", Passphrase);

                    var paymentData = new
                    {
                        amount = payment.AmountPaid,
                        currency = "ZAR",
                    };
                    var paymentJson = Newtonsoft.Json.JsonConvert.SerializeObject(paymentData);
                    var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");

                    var response = httpClient.PostAsync("payment", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing payment: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
