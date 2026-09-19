using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.PaymentGateway;
using SkyBook.Data.Data;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGateway _paymentGateway;
        public PaymentService(ApplicationDbContext context, IPaymentGateway paymentGateway)
        {
            _context= context;
            _paymentGateway= paymentGateway;
        }
        public async Task<PaymentResult> ProcessPaymentAsync(int bookingId, PaymentMethod paymentMethod)
        {
            var booking = await _context.Bookings.Include(x => x.Payment).FirstOrDefaultAsync(x => x.Id == bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }
            if (booking.Status != BookingStatus.PendingPayment)
            {
                throw new Exception("Booking already processd");
            }
           var result= await _paymentGateway.ProcessPaymentAsync(booking.TotalPrice,paymentMethod);
            if (result.IsSuccess)
            {
                booking.Status = BookingStatus.Confirmed;
                booking.Payment.PaymentStatus = PaymentStatus.Paid;
                booking.Payment.TransactionId = result.TransactionId;
                booking.Payment.PaymentMethod = paymentMethod;
                booking.Payment.PaidAt = DateTime.Now;
            }
            else
            {
                booking.Payment.PaymentStatus = PaymentStatus.Failed;
                booking.Payment.PaymentMethod = paymentMethod;
            }
            await _context.SaveChangesAsync();
            return result;
        }
    }
}
