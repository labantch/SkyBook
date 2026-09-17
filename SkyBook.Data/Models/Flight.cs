using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Models
{
    public enum FlightStatus
    {
        Scheduled = 1,
        Delayed = 2,
        Boarding = 3,
        Departed = 4,
        Landed = 5,
        Cancelled = 6
    }
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int AircraftId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public FlightStatus Status { get; set; }
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
        public Aircraft Aircraft { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
