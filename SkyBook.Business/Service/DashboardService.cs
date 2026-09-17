using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class DashboardService : IDashboardService
{
       private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }
    #region GetDashboardData
    public async Task<DashboardVM> GetDashboardDataAsync()
        {
            var totalUsers =await _context.Users.CountAsync();
            var totalFlights =await _context.Flights.CountAsync();
            var totalBookings =await _context.Bookings.CountAsync();
            var totalPassengers =await _context.Passengers.CountAsync();
            var totalAircrafts =await _context.Aircrafts.CountAsync();
            var totalSeats =await _context.Seats.CountAsync();
            var bookedSeats =await _context.Bookings.CountAsync(b =>b.Status != BookingStatus.Cancelled);

            var availableSeats = totalSeats - bookedSeats;

            var recentBookings =await _context.Bookings
                    .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                    .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                    .Include(b => b.Seat)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .Select(b => new MyBookingVM
                    {
                        BookingId = b.Id,
                        BookingReference = b.BookingReference,
                        FlightNumber = b.Flight.FlightNumber,
                        DepartureAirPort = b.Flight.DepartureAirport.Name,
                        ArrivalAirPort = b.Flight.ArrivalAirport.Name,
                        DepartureTime = b.Flight.DepartureTime,
                        SeatNumber = b.Seat.SeatNumber,
                        Status = b.Status
                    })
                    .ToListAsync();

            return new DashboardVM
            {
                TotalUsers = totalUsers,
                TotalFlights = totalFlights,
                TotalBookings = totalBookings,
                TotalPassengers = totalPassengers,
                TotalAircrafts = totalAircrafts,
                AvailableSeats = availableSeats,
                ResentBookings = recentBookings
            };
        }
    #endregion
}







