namespace SkyBook.Business.ViewModels
{
    public class DashboardVM
    {
        public int TotalUsers {  get; set; }
        public int TotalFlights { get; set; }
        public int TotalBookings { get; set; }
        public int TotalAircrafts { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalPassengers {  get; set; }
        public List<MyBookingVM> ResentBookings {  get; set; }=new List<MyBookingVM>();

    }
}
