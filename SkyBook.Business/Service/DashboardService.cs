using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyBook.Business.Service;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingService _bookingService;

    public DashboardService(ApplicationDbContext context, IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    public async Task<DashboardVM> GetDashboardDataAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalFlights = await _context.Flights.CountAsync();
        var totalPassengers = await _context.Passengers.CountAsync();
        var totalAircrafts = await _context.Aircrafts.CountAsync();
        var totalAirports = await _context.Airports.CountAsync();
        var totalSeats = await _context.Seats.CountAsync();
        var bookedSeats = await _context.Bookings.CountAsync(b => b.Status != BookingStatus.Cancelled);

        var availableSeats = Math.Max(0, totalSeats - bookedSeats);

        // Grouped bookings identical to Admin Bookings management page
        var allBookings = await _bookingService.GetAllBookingsAsync();
        var totalBookings = allBookings.Count;
        var recentBookings = allBookings.Take(6).ToList();

        // Card 1
        var inFlightCount = await _context.Flights.CountAsync(f => f.Status == FlightStatus.Departed || f.Status == FlightStatus.Boarding);
        var scheduledFlightsCount = await _context.Flights.CountAsync(f => f.Status == FlightStatus.Scheduled);
        var delayedFlightsCount = await _context.Flights.CountAsync(f => f.Status == FlightStatus.Delayed);

        // Card 2
        var totalCountriesCount = await _context.Airports.Where(a => !string.IsNullOrEmpty(a.Country)).Select(a => a.Country).Distinct().CountAsync();
        var totalCitiesCount = await _context.Airports.Where(a => !string.IsNullOrEmpty(a.City)).Select(a => a.City).Distinct().CountAsync();

        // Card 3
        var aircraftModels = await _context.Aircrafts
            .Where(a => !string.IsNullOrEmpty(a.Name))
            .Select(a => a.Name)
            .Distinct()
            .Take(4)
            .ToListAsync();

        // Card 4
        var adminRoleId = await _context.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
        var adminCount = adminRoleId != null ? await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRoleId) : 0;
        var standardUserCount = Math.Max(0, totalUsers - adminCount);

        // Card 5
        var totalRevenueGross = await _context.Bookings.Where(b => b.Status != BookingStatus.Cancelled).SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;
        var confirmedBookingsCount = allBookings.Count(b => b.Status == BookingStatus.Confirmed);
        double seatLoadPercentage = totalSeats > 0 ? Math.Round((double)bookedSeats / totalSeats * 100, 1) : 0;

        var recentFlightsEntities = await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .OrderByDescending(f => f.DepartureTime)
            .Take(6)
            .ToListAsync();

        var recentFlights = recentFlightsEntities.Select(f => new FlightVM
        {
            Id = f.Id,
            FlightNumber = f.FlightNumber ?? "",
            DepartureAirportCode = f.DepartureAirport?.Code ?? f.DepartureAirport?.Name ?? "",
            DepartureAirportName = f.DepartureAirport?.Name ?? "",
            DepartureAirportCity = f.DepartureAirport?.City ?? "",
            ArrivalAirportCode = f.ArrivalAirport?.Code ?? f.ArrivalAirport?.Name ?? "",
            ArrivalAirportName = f.ArrivalAirport?.Name ?? "",
            ArrivalAirportCity = f.ArrivalAirport?.City ?? "",
            AircraftName = f.Aircraft?.Name ?? "",
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.ArrivalTime,
            Price = f.Price,
            Status = f.Status
        }).ToList();

        return new DashboardVM
        {
            TotalUsers = totalUsers,
            TotalFlights = totalFlights,
            TotalBookings = totalBookings,
            TotalPassengers = totalPassengers,
            TotalAircrafts = totalAircrafts,
            TotalAirports = totalAirports,
            AvailableSeats = availableSeats,
            InFlightCount = inFlightCount,
            ScheduledFlightsCount = scheduledFlightsCount,
            DelayedFlightsCount = delayedFlightsCount,
            TotalCountriesCount = totalCountriesCount,
            TotalCitiesCount = totalCitiesCount,
            AircraftModels = aircraftModels,
            AdminCount = adminCount,
            StandardUserCount = standardUserCount,
            TotalRevenueGross = totalRevenueGross,
            ConfirmedBookingsCount = confirmedBookingsCount,
            SeatLoadPercentage = seatLoadPercentage,
            RecentBookings = recentBookings,
            RecentFlights = recentFlights
        };
    }
}
