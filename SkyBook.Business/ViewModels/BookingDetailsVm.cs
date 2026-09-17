using SkyBook.Data.Models;

namespace SkyBook.Business.ViewModels
{
    public class BookingDetailsVm
    {
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string BookingReference { get; set; }
        public string DepartureAirPort { get; set; }
        public string ArrivalAirPort { get; set; }
        public string PassengerName {  get; set; }
        public string FlightNumber {  get; set; }
        public string SeatNumber {  get; set; }
        public string? UserImageUrl { get; set; }
    }
}
