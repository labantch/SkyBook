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
        return await _context.Flights
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .Include(f => f.Stops)
                .ThenInclude(s => s.Airport)
            .Select(f => new FlightVM
            {
                Id = f.Id,
                FlightNumber = f.FlightNumber,
                AircraftId = f.AircraftId,
                DepartureAirportId = f.DepartureAirportId,
                ArrivalAirportId = f.ArrivalAirportId,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Price = f.Price,
                EconomyPrice = f.EconomyPrice,
                BusinessPrice = f.BusinessPrice,
                FirstClassPrice = f.FirstClassPrice,
                Status = f.Status,
                DepartureAirportCode = f.DepartureAirport.Code,
                DepartureAirportName = f.DepartureAirport.Name,
                DepartureAirportCity = f.DepartureAirport.City,
                ArrivalAirportCode = f.ArrivalAirport.Code,
                ArrivalAirportName = f.ArrivalAirport.Name,
                ArrivalAirportCity = f.ArrivalAirport.City,
                AircraftName = f.Aircraft.Name,
                AvailableSeats = f.Aircraft.Seats.Count - f.Bookings.Count(b => b.Status != BookingStatus.Cancelled),
                Stops = f.Stops
                    .OrderBy(s => s.StopOrder)
                    .Select(s => new FlightStopVM
                    {
                        Id = s.Id,
                        FlightId = s.FlightId,
                        AirportId = s.AirportId,
                        AirportCode = s.Airport.Code,
                        AirportName = s.Airport.Name,
                        AirportCity = s.Airport.City,
                        StopOrder = s.StopOrder
                    }).ToList()
            }).ToListAsync();

    }
    #endregion

    #region GetFlightById
    public async Task<FlightDetailsVM?> GetFlightByIdAsync(int id)
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
            EconomyPrice = flights.EconomyPrice,
            BusinessPrice = flights.BusinessPrice,
            FirstClassPrice = flights.FirstClassPrice,
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
            throw new Exception("Departure and Arrival airports cannot be the same.");
        }
        if (model.ArrivalTime <= model.DepartureTime)
        {
            throw new Exception("Arrival time must be after departure time.");
        }
        var departureAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == model.DepartureAirportId);
        if (departureAirport == null)
        {
            throw new Exception("Departure airport station could not be found.");
        }
        if (departureAirport.Status != AirportStatus.Active)
        {
            throw new Exception($"Cannot schedule flight: Departure airport '{departureAirport.Name}' ({departureAirport.Code}) is currently {departureAirport.Status} and not operational.");
        }

        var arrivalAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == model.ArrivalAirportId);
        if (arrivalAirport == null)
        {
            throw new Exception("Arrival airport station could not be found.");
        }
        if (arrivalAirport.Status != AirportStatus.Active)
        {
            throw new Exception($"Cannot schedule flight: Arrival airport '{arrivalAirport.Name}' ({arrivalAirport.Code}) is currently {arrivalAirport.Status} and not operational.");
        }

        var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == model.AircraftId);
        if (aircraft == null)
        {
            throw new Exception("Selected aircraft could not be found in active fleet.");
        }
        if (aircraft.Status != AircraftStatus.Active)
        {
            throw new Exception($"Cannot schedule flight: Aircraft '{aircraft.Name}' is currently {aircraft.Status} and cannot be assigned to flights.");
        }

        var flightNumberExists = await _context.Flights.AnyAsync(f => f.FlightNumber == model.FlightNumber);
        if (flightNumberExists)
        {
            throw new Exception($"Flight number '{model.FlightNumber}' already exists.");
        }

        var flight = new Flight
        {
            FlightNumber = model.FlightNumber.Trim().ToUpper(),
            AircraftId = model.AircraftId,
            DepartureAirportId = model.DepartureAirportId,
            ArrivalAirportId = model.ArrivalAirportId,
            DepartureTime = model.DepartureTime,
            ArrivalTime = model.ArrivalTime,
            Price = model.Price > 0 ? model.Price : model.EconomyPrice,
            EconomyPrice = model.EconomyPrice,
            BusinessPrice = model.BusinessPrice,
            FirstClassPrice = model.FirstClassPrice,
            Status = model.Status != 0 ? model.Status : FlightStatus.Scheduled
        };

        AssignFlightStops(flight, model.Stops, model.DepartureAirportId, model.ArrivalAirportId);

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

    }
    #endregion
    
    #region UpdateFlight
    public async Task UpdateFlightAsync(FlightVM model)
    {
        var flight = await _context.Flights
            .Include(f => f.Stops)
            .FirstOrDefaultAsync(f => f.Id == model.Id);

        if (flight == null)
            throw new Exception("Flight not found.");

        if (model.DepartureAirportId == model.ArrivalAirportId)
            throw new Exception("Departure and Arrival airports cannot be the same.");

        if (model.ArrivalTime <= model.DepartureTime)
            throw new Exception("Arrival time must be after departure time.");

        var departureAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == model.DepartureAirportId);
        if (departureAirport == null)
        {
            throw new Exception("Departure airport station could not be found.");
        }
        if (departureAirport.Status != AirportStatus.Active)
        {
            throw new Exception($"Cannot schedule flight: Departure airport '{departureAirport.Name}' ({departureAirport.Code}) is currently {departureAirport.Status} and not operational.");
        }

        var arrivalAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == model.ArrivalAirportId);
        if (arrivalAirport == null)
        {
            throw new Exception("Arrival airport station could not be found.");
        }
        if (arrivalAirport.Status != AirportStatus.Active)
        {
            throw new Exception($"Cannot schedule flight: Arrival airport '{arrivalAirport.Name}' ({arrivalAirport.Code}) is currently {arrivalAirport.Status} and not operational.");
        }

        var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == model.AircraftId);
        if (aircraft == null)
        {
            throw new Exception("Selected aircraft could not be found in active fleet.");
        }
        if (aircraft.Status != AircraftStatus.Active)
        {
            throw new Exception($"Cannot schedule flight: Aircraft '{aircraft.Name}' is currently {aircraft.Status} and cannot be assigned to flights.");
        }

        var flightNumberExists = await _context.Flights
            .AnyAsync(f =>
                f.FlightNumber == model.FlightNumber &&
                f.Id != model.Id);

        if (flightNumberExists)
            throw new Exception("Flight number already exists.");

        flight.FlightNumber = model.FlightNumber.Trim().ToUpper();
        flight.DepartureAirportId = model.DepartureAirportId;
        flight.ArrivalAirportId = model.ArrivalAirportId;
        flight.AircraftId = model.AircraftId;
        flight.DepartureTime = model.DepartureTime;
        flight.ArrivalTime = model.ArrivalTime;
        flight.Price = model.Price > 0 ? model.Price : model.EconomyPrice;
        flight.EconomyPrice = model.EconomyPrice;
        flight.BusinessPrice = model.BusinessPrice;
        flight.FirstClassPrice = model.FirstClassPrice;
        flight.Status = model.Status;

        // Synchronize intermediate stops
        AssignFlightStops(flight, model.Stops, model.DepartureAirportId, model.ArrivalAirportId);

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
                    EconomyPrice = flight.EconomyPrice,
                    BusinessPrice = flight.BusinessPrice,
                    FirstClassPrice = flight.FirstClassPrice,
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
            .Include(f => f.DepartureAirport)
            .Include(f => f.ArrivalAirport)
            .Include(f => f.Aircraft)
            .Include(f => f.Stops)
                .ThenInclude(s => s.Airport)
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
                EconomyPrice = f.EconomyPrice,
                BusinessPrice = f.BusinessPrice,
                FirstClassPrice = f.FirstClassPrice,
                Status = f.Status,
                DepartureAirportCode = f.DepartureAirport.Code,
                DepartureAirportName = f.DepartureAirport.Name,
                DepartureAirportCity = f.DepartureAirport.City,
                ArrivalAirportCode = f.ArrivalAirport.Code,
                ArrivalAirportName = f.ArrivalAirport.Name,
                ArrivalAirportCity = f.ArrivalAirport.City,
                AircraftName = f.Aircraft.Name,
                Stops = f.Stops
                    .OrderBy(s => s.StopOrder)
                    .Select(s => new FlightStopVM
                    {
                        Id = s.Id,
                        FlightId = s.FlightId,
                        AirportId = s.AirportId,
                        AirportCode = s.Airport.Code,
                        AirportName = s.Airport.Name,
                        AirportCity = s.Airport.City,
                        StopOrder = s.StopOrder
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }
    #endregion

    private static void AssignFlightStops(Flight flight, List<FlightStopVM>? stops, int departureAirportId, int arrivalAirportId)
    {
        flight.Stops.Clear();
        if (stops == null || !stops.Any()) return;

        int order = 1;
        var seen = new HashSet<int>();
        foreach (var stop in stops.Where(s => s.AirportId > 0))
        {
            if (stop.AirportId == departureAirportId || stop.AirportId == arrivalAirportId)
                continue;
            if (!seen.Add(stop.AirportId))
                continue;

            flight.Stops.Add(new FlightStop
            {
                AirportId = stop.AirportId,
                StopOrder = order++
            });
        }
    }
}
















