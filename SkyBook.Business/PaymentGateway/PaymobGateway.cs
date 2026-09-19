using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.PaymentGateway
{
    public class PaymobGateway : IPaymentGateway
    {
        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentMethod paymentMethod)
        {
            await Task.Delay(1000);
            return new PaymentResult
            {
                IsSuccess = true,
                TransactionId = Guid.NewGuid().ToString(),
                Message = "payment Successful"
            };
        }
    }
}
