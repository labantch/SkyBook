namespace SkyBook.Business.ViewModels
{
    public class FlightStopVM
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public int AirportId { get; set; }
        public string? AirportCode { get; set; }
        public string? AirportName { get; set; }
        public string? AirportCity { get; set; }
        public int StopOrder { get; set; }
    }
}
