using SkyBook.Data.Models;

namespace SkyBook.Presentation.ViewModels
{
    public class MyBookingVM
    {
        public int BookingId {  get; set; }
        public string BookingReference { get; set; }
        public string FlightNumber {  get; set; }
        public string DepartureAirPort { get; set; }
        public string ArrivalAirPort { get; set; }
        public DateTime DepartureTime {  get; set; }
        public string SeatNumber {  get; set; }
        public BookingStatus bookingStuatus {  get; set; }
    }
}
