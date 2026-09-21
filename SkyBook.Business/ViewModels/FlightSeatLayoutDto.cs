using System.Collections.Generic;

namespace SkyBook.Business.ViewModels
{
    public class SeatDetailDto
    {
        public string SeatNumber { get; set; } = string.Empty;
        public string CabinClass { get; set; } = string.Empty; 
        public int Row { get; set; }
        public string Letter { get; set; } = string.Empty;
    }

    public class FlightSeatLayoutDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AircraftName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int FirstClassSeats { get; set; }
        public int BusinessSeats { get; set; }
        public int EconomySeats { get; set; }
        public List<SeatDetailDto> Seats { get; set; } = new();
        public List<string> OccupiedSeats { get; set; } = new();
    }
}
