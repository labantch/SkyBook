using SkyBook.Business.PaymentGateway;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.Interfaces
{
    public interface IPaymentGateway
    {
        public Task<PaymentResult>ProcessPaymentAsync(decimal amount,PaymentMethod paymentMethod);
    }
}
