using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class FlightService : IFlightService
{
    private ApplicationDbContext context = new ApplicationDbContext();
    public async Task<List<FlightVM>> GetAllFlightsAsync()
    {
        var flights = await context.Flights.ToListAsync();
        var result = new List<FlightVM>();
        foreach (var flight in flights)
        {
            var f = new FlightVM()
            {
                Id = flight.Id,
                FlightNumber = flight.FlightNumber,
                AircraftID = flight.AircraftId,
                DepartureAirportId = flight.DepartureAirportId,
                ArrivalAirportId = flight.ArrivalAirportId,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                Price = flight.Price,
                Status = flight.Status
            };
            result.Add(f);
        }

        return result;
    }

    public async Task<FlightDetailsVM> GetFlightsByIdAsync(int id)
    {
        var flights = await context.Flights
            .Include(flight => flight.Aircraft)
            .Include(flight => flight.DepartureAirport)
            .Include(flight => flight.ArrivalAirport)
            .FirstOrDefaultAsync(f => f.Id == id);
        var flight = new FlightDetailsVM()
        {
            FlightId = flights.Id,
            FlightNumber = flights.FlightNumber,
            AircraftName = flights.Aircraft.Name,
            DepartureAirport = flights.DepartureAirport.Name,
            ArrivalAirport = flights.ArrivalAirport.Name,
            DepartureTime = flights.DepartureTime,
            ArrivalTime = flights.ArrivalTime,
            Price = flights.Price,
            Status = flights.Status
        };
        return flight;
    }

    public async Task CreatAsync(FlightVM flight)
    {
        bool flightExist = await context.Flights.AnyAsync(a => a.Id == flight.Id);
        if (flightExist)
        {
            throw new Exception("Flight Already Exist");
        }
        var flights=new Flight()
        {
            FlightNumber = flight.FlightNumber,
            AircraftId = flight.AircraftID,
            DepartureAirportId = flight.DepartureAirportId,
            ArrivalAirportId = flight.ArrivalAirportId,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Price = flight.Price
        };
        await context.Flights.AddAsync(flights);
        await context.SaveChangesAsync();
        
    }

    public async Task EditAsync(FlightVM flight)
    {
        var flights = await context.Flights.FirstOrDefaultAsync(a => a.FlightNumber ==flight.FlightNumber);
        if (flights == null)
        {
            throw new Exception("Aircraft  Not Found");
        }
        flights.FlightNumber = flight.FlightNumber;
        flights.DepartureAirportId = flight.DepartureAirportId;
        flights.ArrivalAirportId = flight.ArrivalAirportId;
        flights.AircraftId = flight.AircraftID;
        flights.DepartureTime = flight.DepartureTime;
        flights.ArrivalTime = flight.ArrivalTime;
        flights.Price = flight.Price;
        flights.Status = flight.Status;
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        bool hasFlights = await context.Flights.AnyAsync(a => a.Id == id);
        if (hasFlights)
        {
            throw new Exception("Aircraft is used in Flights");
        }
        var flight = await context.Aircrafts.FindAsync(id);
        if (flight == null)
        {
            throw new Exception("Aircraft Not  Found");
        }
        context.Aircrafts.Remove(flight);
        await context.SaveChangesAsync();
    }

    public async Task<FlightSearshVM> Search(int id)
    {
        var flights = await context.Flights
            .Include(flight => flight.Aircraft)
            .Include(flight => flight.DepartureAirport)
            .Include(flight => flight.ArrivalAirport)
            .FirstOrDefaultAsync(f => f.Id == id);
        var flight = new FlightSearshVM()
        {
            DepartureAirPortId = flights.DepartureAirportId,
            ArrivalAirPortId = flights.ArrivalAirportId,
            TravelDate = flights.DepartureTime,
            PassengerCount = flights.Aircraft.Capacity
        };
        return flight;
    }

    public async Task<FlightVM> ChangeStatusAsync(int id, FlightStatus status)
    {
        var flight = await context.Flights
            .FirstOrDefaultAsync(f => f.Id == id);
        if (flight == null)
            return null;
        flight.Status = status;
        await context.SaveChangesAsync();
        return new FlightVM()
        {
            Id = flight.Id,
            Status = flight.Status
        };
    }
    
    public async Task<FlightCardVM> GetFlightCardAsync(int id)
    {
        var flights = await context.Flights
            .Include(flight => flight.Aircraft)
            .Include(flight => flight.DepartureAirport)
            .Include(flight => flight.ArrivalAirport)
            .FirstOrDefaultAsync(f => f.Id == id);
        var flight = new FlightCardVM()
        {
            FlightID = flights.Id,
            FlightNumber = flights.FlightNumber,
            ArrivalAirPort = flights.ArrivalAirport.Name,
            ArrivalTime = flights.ArrivalTime,
            DepartureAirPort = flights.DepartureAirport.Name,
            DepartureTime = flights.DepartureTime,
            Price = flights.Price,
            // AvailableSeat = flights.Aircraft.Seats.

        };
        return flight;
    }
}