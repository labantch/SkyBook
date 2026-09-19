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
            
            if (amount <= 0)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    TransactionId = null,
                    Message = "Payment Failed"
                };
            }
            
            return new PaymentResult
            {
                IsSuccess = true,
                TransactionId = Guid.NewGuid().ToString(),
                Message = "payment Successful"
            };
            
        }
    }
}
