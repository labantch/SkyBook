using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class DashboardService : IDashboardService
{
    private ApplicationDbContext context = new ApplicationDbContext();
    public async Task<DashboardVM> GetTotalUsersAsync()
    {
        var users = await context.ApplicationUsers.ToListAsync();
        var user = new DashboardVM()
        {
            TotalAircrafts = users.Count,
        };
        return user;

    }

    public async Task<DashboardVM> GetTotalFlightsAsync()
    {
        var flights = await context.Flights.ToListAsync();
        var flight = new DashboardVM()
        {
            TotalFlights = flights.Count,
        };
        return flight;
    }

    public async Task<DashboardVM> GetTotalBookingsAsync()
    {
        var books = await context.Bookings.ToListAsync();
        var book = new DashboardVM()
        {
            TotalBooking = books.Count,
        };
        return book;
    }

    public async Task<DashboardVM> GetTotalAircraftAsync()
    {
        var aircrafts = await context.Aircrafts.ToListAsync();
        var aircraft = new DashboardVM()
        {
            TotalAircrafts = aircrafts.Count,
        };
        return aircraft;
    }

    public async Task<DashboardVM> GetAvailableSeatsAsync(int id)
    {
        var seats = await context.Seats.ToListAsync();
        var seat = new DashboardVM()
        {
            AvailableSeats = seats.Count,
        };
        return seat;
    }
}