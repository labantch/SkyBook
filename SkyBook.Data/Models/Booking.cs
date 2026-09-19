using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Models
{
    public enum BookingStatus
    {
        PendingPayment = 1,
        Confirmed = 2,
        Cancelled = 3
    }
    public class Booking
    {
        public int Id { get; set; }  
        public string UserId { get; set; }
        public int PassengerId { get; set; }
        public int FlightId { get; set; }
        public int SeatId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string BookingReference { get; set; }
        public ApplicationUser User { get; set; }
        public Passenger Passenger { get; set; }
        public Flight Flight { get; set; }
        public Seat Seat { get; set; }
        public Payment Payment { get; set; }
    }
}
