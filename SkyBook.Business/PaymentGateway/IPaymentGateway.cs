using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.PaymentGateway
{
    public interface IPaymentGateway
    {
        public Task<PaymentResult>ProcessPaymentAsync(PaymentRequest request);
    }
}
