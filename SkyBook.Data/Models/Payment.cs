using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Models
{
    public enum PaymentMethod
    {
        Card,
        Wallet
    }
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Faild,
        Refunded
    }
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Booking Booking { get; set; }
    }
 
    
}
