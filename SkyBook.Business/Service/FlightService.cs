using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class FlightService : IFlightService
{
    private ApplicationDbContext _context;
    public FlightService(ApplicationDbContext context)
    {
        _context = context;
    }
    #region GetAllFlights
    public async Task<List<FlightVM>> GetAllFlightsAsync()
    {
        return await _context.Flights.Select(f => new FlightVM
        {
            Id = f.Id,
            FlightNumber = f.FlightNumber,

            AircraftId = f.AircraftId,
            DepartureAirportId = f.DepartureAirportId,
            ArrivalAirportId = f.ArrivalAirportId,
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.ArrivalTime,
            Price = f.Price,
            Status = f.Status
        }).ToListAsync();

    }
    #endregion

    #region GetFlightById
    public async Task<FlightDetailsVM> GetFlightByIdAsync(int id)
    {
        var flights = await _context.Flights
            .Include(f => f.Aircraft)
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (flights == null)
        {
            return null;
        }
        var seats = await _context.Seats.Where(s => s.AircraftId == flights.AircraftId)
        .Select(s => new SeatSelectionVM
        {
            SeatId = s.Id,
            SeatNumber = s.SeatNumber,
            SeatClass = s.Class,
            IsBooked = _context.Bookings
        .Any(b => b.FlightId == flights.Id && b.SeatId == s.Id)
        }).ToListAsync();
        var avaliablesSeats = seats.Count(c => !c.IsBooked);
        return new FlightDetailsVM
        {
            FlightId = flights.Id,
            FlightNumber = flights.FlightNumber,
            AircraftName = flights.Aircraft.Name,
            ArrivalAirport = flights.ArrivalAirport.Name,
            DepartureAirport = flights.DepartureAirport.Name,
            Price = flights.Price,
            Status = flights.Status,
            AvaliableSeats = avaliablesSeats,
            Seats = seats
        };
    }
    #endregion
    
    #region  CreateFlight
    public async Task CreateFlightAsync(FlightVM model)
    {
        if (model.DepartureAirportId == model.ArrivalAirportId)
        {
            throw new Exception("Departure and Arrival airport cannot be the same");
        }
        if (model.ArrivalTime <= model.DepartureTime)
        {
            throw new Exception("Arival time must be after departure time");
        }
        bool departureAirport=await _context.Airports.AnyAsync(a=>a.Id == model.DepartureAirportId);
        bool arrivalAirport=await _context.Airports.AnyAsync(a=>a.Id == model.ArrivalAirportId);

        if (!departureAirport&&arrivalAirport)
        {
            throw new Exception("airport not found");
        }
        var flight = new Flight
        {

            FlightNumber = model.FlightNumber,
            AircraftId = model.AircraftId,
            DepartureAirportId = model.DepartureAirportId,
            ArrivalAirportId = model.ArrivalAirportId,
            DepartureTime = model.DepartureTime,
            ArrivalTime = model.ArrivalTime,
            Price = model.Price
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

    }
    #endregion
    
    #region UpdateFlight
    public async Task UpdateFlightAsync(FlightVM model)
    {
        var flight = await _context.Flights
            .FirstOrDefaultAsync(f => f.Id == model.Id);

        if (flight == null)
            throw new Exception("Flight not found.");

        if (model.DepartureAirportId == model.ArrivalAirportId)
            throw new Exception("Departure and Arrival airports cannot be the same.");

        if (model.ArrivalTime <= model.DepartureTime)
            throw new Exception("Arrival time must be after departure time.");

        var flightNumberExists = await _context.Flights
            .AnyAsync(f =>
                f.FlightNumber == model.FlightNumber &&
                f.Id != model.Id);

        if (flightNumberExists)
            throw new Exception("Flight number already exists.");

        flight.FlightNumber = model.FlightNumber;
        flight.DepartureAirportId = model.DepartureAirportId;
        flight.ArrivalAirportId = model.ArrivalAirportId;
        flight.AircraftId = model.AircraftId;
        flight.DepartureTime = model.DepartureTime;
        flight.ArrivalTime = model.ArrivalTime;
        flight.Price = model.Price;
        flight.Status = model.Status;

        await _context.SaveChangesAsync();
    }

    #endregion
    
    #region DeleteFlight
    public async Task DeleteFlightAsync(int id)
    {
        var flight = await _context.Flights
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flight == null)
            throw new Exception("Flight not found.");

        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.FlightId == id);

        if (hasBookings)
            throw new Exception("Cannot delete a flight that has bookings.");

        _context.Flights.Remove(flight);

        await _context.SaveChangesAsync();
    }
    #endregion
    
    #region SearchFlights
    public async Task<List<FlightCardVM>> SearchFlightsAsync(
          FlightSearshVM model)
    {
        var flights = await _context.Flights
            .Where(f =>
                f.DepartureAirportId == model.DepartureAirportId &&
                f.ArrivalAirportId == model.ArrivalAirportId &&
                f.DepartureTime.Date == model.TravelDate.Date
            )
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .ToListAsync();

        var result = new List<FlightCardVM>();

        foreach (var flight in flights)
        {
            var totalSeats = await _context.Seats
                .CountAsync(s => s.AircraftId == flight.AircraftId);
            var bookedSeats = await _context.Bookings
                .CountAsync(b => b.FlightId == flight.Id);
            var availableSeats = totalSeats - bookedSeats;
            if (availableSeats >= model.PassengerCount)
            {
                result.Add(new FlightCardVM
                {
                    FlightID = flight.Id,
                    FlightNumber = flight.FlightNumber,
                    DepartureAirPort = flight.DepartureAirport.Name,
                    ArrivalAirPort = flight.ArrivalAirport.Name,

                    DepartureTime = flight.DepartureTime,
                    ArrivalTime = flight.ArrivalTime,

                    Price = flight.Price,
                    AvailableSeat = availableSeats
                });
            }
        }

        return result; }
    #endregion
    
    #region ChangeStatus
    public async Task ChangeStatusAsync(int flightId,FlightStatus status)
    {
        var flight = await _context.Flights
            .FirstOrDefaultAsync(f => f.Id == flightId);
        if (flight == null)
            throw new Exception("Flight not found.");

        flight.Status = status;
        await _context.SaveChangesAsync();
    }
    #endregion
    
    #region GetFlightForEdit
    public async Task<FlightVM?> GetFlightForEditAsync(int id)
    {
        return await _context.Flights
            .Where(f => f.Id == id)
            .Select(f => new FlightVM
            {
                Id = f.Id,
                FlightNumber = f.FlightNumber,
                DepartureAirportId = f.DepartureAirportId,
                ArrivalAirportId = f.ArrivalAirportId,
                AircraftId = f.AircraftId,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Price = f.Price,
                Status = f.Status
            })
            .FirstOrDefaultAsync();
    }
    #endregion
}
















