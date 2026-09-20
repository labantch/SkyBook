using System.Collections.Generic;

namespace SkyBook.Business.ViewModels
{
    public class ConfirmBookingDto
    {
        public string? BookingReference { get; set; }
        public decimal TotalPrice { get; set; }
        public string? PaymentMethod { get; set; }
        public List<ConfirmFlightSegmentDto> Segments { get; set; } = new();
        public List<ConfirmPassengerDto> Passengers { get; set; } = new();
    }

    public class ConfirmFlightSegmentDto
    {
        public int? FlightId { get; set; }
        public string? FlightNumber { get; set; }
        public string? FromCode { get; set; }
        public string? ToCode { get; set; }
        public decimal Price { get; set; }
        public string? Aircraft { get; set; }
    }

    public class ConfirmPassengerDto
    {
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Dob { get; set; }
        public string? Nationality { get; set; }
        public string? Passport { get; set; }
        public string? SeatOutbound { get; set; }
        public string? SeatReturn { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
