using SkyBook.Business.ViewModels;
using System.Collections.Generic;

namespace SkyBook.Business.ViewModels
{
    public class DashboardVM
    {
        // 5 Primary KPI Cards
        public int TotalFlights { get; set; }
        public int InFlightCount { get; set; }
        public int ScheduledFlightsCount { get; set; }
        public int DelayedFlightsCount { get; set; }

        public int TotalAirports { get; set; }
        public int TotalCountriesCount { get; set; }
        public int TotalCitiesCount { get; set; }

        public int TotalAircrafts { get; set; }
        public List<string> AircraftModels { get; set; } = new List<string>();

        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int StandardUserCount { get; set; }

        public int TotalBookings { get; set; }
        public decimal TotalRevenueGross { get; set; }
        public int ConfirmedBookingsCount { get; set; }
        public double SeatLoadPercentage { get; set; }

        // Extra KPI metrics
        public int AvailableSeats { get; set; }
        public int TotalPassengers { get; set; }

        // Data Tables
        public List<BookingDetailsVm> RecentBookings { get; set; } = new List<BookingDetailsVm>();
        public List<FlightVM> RecentFlights { get; set; } = new List<FlightVM>();

        // Legacy compatibility
        public List<MyBookingVM> ResentBookings { get; set; } = new List<MyBookingVM>();
    }
}
