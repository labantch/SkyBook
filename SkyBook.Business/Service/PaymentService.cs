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
            if (booking.Status == BookingStatus.Cancelled)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "A cancelled booking cannot be paid. Please create a new reservation."
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
           
            var baseRef = !string.IsNullOrWhiteSpace(booking.BookingReference) && booking.BookingReference.Contains('-')
                ? System.Text.RegularExpressions.Regex.Replace(booking.BookingReference.Trim(), @"-\d+$", "")
                : booking.BookingReference;

            var relatedBookings = await _context.Bookings
                .Where(b => b.BookingReference == baseRef || b.BookingReference.StartsWith(baseRef + "-"))
                .ToListAsync();

            decimal totalAmount = relatedBookings.Sum(b => b.TotalPrice);
            if (totalAmount <= 0)
            {
                totalAmount = booking.TotalPrice;
            }

            var request = new PaymentRequest
            {
                Amount = totalAmount,
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
                    Amount = totalAmount,
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
                existingPayment.Amount = totalAmount;
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
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId)
                ?? await _context.payments
                .Include(x => x.Booking)
                .Where(x => x.PaymentStatus == PaymentStatus.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                return;
            }

            payment.TransactionId = transactionId;
            payment.PaymentStatus = status;

            var baseRef = payment.Booking != null && !string.IsNullOrWhiteSpace(payment.Booking.BookingReference) && payment.Booking.BookingReference.Contains('-')
                ? System.Text.RegularExpressions.Regex.Replace(payment.Booking.BookingReference.Trim(), @"-\d+$", "")
                : payment.Booking?.BookingReference;

            var related = await _context.Bookings
                .Where(b => b.BookingReference == baseRef || b.BookingReference.StartsWith(baseRef + "-"))
                .ToListAsync();

            if (status == PaymentStatus.Paid)
            {
                payment.PaidAt = DateTime.UtcNow;
                foreach (var b in related)
                {
                    if (b.Status != BookingStatus.Cancelled)
                        b.Status = BookingStatus.Confirmed;
                }
            }
            if (status == PaymentStatus.Failed)
            {
                foreach (var b in related)
                {
                    b.Status = BookingStatus.Cancelled;
                }
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
