using System;
using System.Collections.Generic;
using SkyBook.Data.Models;

namespace SkyBook.Business.ViewModels
{
    public class BookingDetailsVm
    {
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string DepartureAirPort { get; set; } = string.Empty;
        public string ArrivalAirPort { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string? UserImageUrl { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<PassengerBookingVm> Passengers { get; set; } = new();
        public List<BookingSegmentVm> Segments { get; set; } = new();
        public string TripType => Segments.Count switch
        {
            1 => "One Way",
            2 => "Round Trip",
            _ => "Multi-City"
        };
    }
}
