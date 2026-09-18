using SkyBook.Data.Models;
using System;

namespace SkyBook.Business.ViewModels
{
    public class MyBookingVM
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureAirPort { get; set; } = string.Empty;
        public string ArrivalAirPort { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public BookingStatus bookingStuatus { get => Status; set => Status = value; }
        public decimal TotalPrice { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}
