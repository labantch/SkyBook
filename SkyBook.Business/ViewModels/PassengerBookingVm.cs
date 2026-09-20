namespace SkyBook.Business.ViewModels
{
    public class PassengerBookingVm
    {
        public int PassengerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public string TicketReference { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
