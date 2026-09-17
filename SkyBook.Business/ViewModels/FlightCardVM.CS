using Microsoft.EntityFrameworkCore.Query;

namespace SkyBook.Business.ViewModels
{
    public class FlightCardVM
    {
        public int FlightID {  get; set; }
        public string FlightNumber { get; set; }
        public string DepartureAirPort { get; set; }
        public string ArrivalAirPort { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price {  get; set; }
        public int AvailableSeat {  get; set; }
    }
}
