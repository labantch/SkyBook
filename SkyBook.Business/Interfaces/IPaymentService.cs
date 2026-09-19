using SkyBook.Business.PaymentGateway;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.Interfaces
{
    public  interface IPaymentService
    {
        Task<PaymentResult>ProcessPaymentAsync(int bookingId,PaymentMethod paymentMethod);
        Task UpdatePaymentStatusAsync(String transationId, PaymentStatus status);

    }
}
