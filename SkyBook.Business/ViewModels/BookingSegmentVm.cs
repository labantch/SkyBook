namespace SkyBook.Business.ViewModels
{
    public class BookingSegmentVm
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public string SeatNumbers { get; set; } = string.Empty;
    }
}
