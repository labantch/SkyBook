using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.PaymentGateway;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentService(
            ApplicationDbContext context,
            IPaymentGateway paymentGateway)
        {
            _context = context;
            _paymentGateway = paymentGateway;
        }

        public async Task<PaymentResult> CreatePaymentAsync(
            int bookingId,
            PaymentMethod paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(x => x.Passenger)
                .FirstOrDefaultAsync(x => x.Id == bookingId);

            if (booking == null)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Booking not found."
                };
            }

            if (booking.Status == BookingStatus.Confirmed)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "This booking is already confirmed."
                };
            }
            var existingPayment = await _context.payments
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

            if (existingPayment != null &&
                existingPayment.PaymentStatus == PaymentStatus.Paid)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "This booking has already been paid."
                };
            }
            var request = new PaymentRequest
            {
                Amount = booking.TotalPrice,
                PaymentMethod = paymentMethod,
                CustomerName =
                    $"{booking.Passenger.FirstName} {booking.Passenger.LastName}",
                CustomerEmail = booking.Passenger.Email ?? string.Empty,
                CustomerPhone = booking.Passenger.Phone ?? string.Empty
            };
            var result =
                await _paymentGateway.ProcessPaymentAsync(request);

            if (!result.IsSuccess)
            {
                return result;
            }
            if (existingPayment == null)
            {
                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = booking.TotalPrice,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = PaymentStatus.Pending,
                    TransactionId = result.TransactionId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.payments.Add(payment);
            }
            else
            {
                existingPayment.PaymentMethod = paymentMethod;
                existingPayment.Amount = booking.TotalPrice;
                existingPayment.TransactionId = result.TransactionId;
                existingPayment.PaymentStatus = PaymentStatus.Pending;
            }
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task UpdatePaymentStatusAsync(
            string transactionId,
            PaymentStatus status)
        {
            var payment = await _context.payments
                .Include(x => x.Booking)
                .FirstOrDefaultAsync(
                    x => x.TransactionId == transactionId);

            if (payment == null)
            {
                return;
            }
            payment.PaymentStatus = status;

            if (status == PaymentStatus.Paid)
            {
                payment.PaidAt = DateTime.UtcNow;
                payment.Booking.Status = BookingStatus.Confirmed;
            }
            if (status == PaymentStatus.Failed)
            {
                payment.Booking.Status = BookingStatus.Cancelled;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Payment?> GetPaymentByBookingIdAsync(
            int bookingId)
        {
            return await _context.payments
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);
        }
    }
}