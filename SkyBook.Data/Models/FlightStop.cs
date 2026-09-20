using System;

namespace SkyBook.Data.Models
{
    public class FlightStop
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public Flight Flight { get; set; } = null!;
        public int AirportId { get; set; }
        public Airport Airport { get; set; } = null!;
        public int StopOrder { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public int? LayoverMinutes { get; set; }
        public string? Remarks { get; set; }
    }
}
