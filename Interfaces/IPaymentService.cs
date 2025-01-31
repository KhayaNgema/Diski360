
using MyField.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyField.Interfaces
{
    public interface IPaymentService
    {
        bool ValidatePayment(Payment payment);
        bool ProcessPayment(Payment payment);
    }

}