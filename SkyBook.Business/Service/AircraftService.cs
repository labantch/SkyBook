using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class AircraftService : IAircraftService
{
    private readonly ApplicationDbContext _context;

    public AircraftService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AircraftVM>> GetAllAircraftsAsync()
    {
        return await _context.Aircrafts
            .Select(a => new AircraftVM 
            { 
                Id = a.Id,
                Name = a.Name,
                Capacity = a.Capacity,
                ImageUrl = a.ImageUrl,
                Status = a.Status,
                EconomySeats = a.Seats.Count(s => s.Class == SeatClass.Economy),
                BusinessSeats = a.Seats.Count(s => s.Class == SeatClass.Business),
                FirstClassSeats = a.Seats.Count(s => s.Class == SeatClass.FirstClass)
            })
            .ToListAsync();
    }

    public async Task<AircraftVM> GetAircraftByIdAsync(int id)
    {
        return await _context.Aircrafts
            .Where(a => a.Id == id)
            .Select(a => new AircraftVM 
            { 
                Id = a.Id,
                Name = a.Name,
                Capacity = a.Capacity,
                ImageUrl = a.ImageUrl,
                Status = a.Status,
                EconomySeats = a.Seats.Count(s => s.Class == SeatClass.Economy),
                BusinessSeats = a.Seats.Count(s => s.Class == SeatClass.Business),
                FirstClassSeats = a.Seats.Count(s => s.Class == SeatClass.FirstClass)
            })
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(AircraftVM model)
    {
        if (model.Id > 0 && await _context.Aircrafts.AnyAsync(a => a.Id == model.Id))
        {
            throw new Exception("An aircraft with this ID already exists.");
        }

        bool nameExists = await _context.Aircrafts.AnyAsync(a => a.Name.ToLower() == model.Name.Trim().ToLower());
        if (nameExists)
        {
            throw new Exception($"An aircraft named '{model.Name}' already exists in the fleet.");
        }

        int seatTotal = model.EconomySeats + model.BusinessSeats + model.FirstClassSeats;
        if (seatTotal > 0)
        {
            model.Capacity = seatTotal;
        }
        else if (model.Capacity > 0)
        {
            model.EconomySeats = model.Capacity;
        }

        var aircraft = new Aircraft
        {
            Name = model.Name.Trim(),
            Capacity = model.Capacity,
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim(),
            Status = model.Status
        };

        var seats = GenerateSeatsForAircraft(model.FirstClassSeats, model.BusinessSeats, model.EconomySeats, model.Capacity);
        aircraft.Seats = seats;

        _context.Aircrafts.Add(aircraft);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AircraftVM model)
    {
        var aircraft = await _context.Aircrafts
            .Include(a => a.Seats)
            .FirstOrDefaultAsync(a => a.Id == model.Id);

        if (aircraft == null)
        {
            throw new Exception("Aircraft Not Found");
        }

        bool nameExists = await _context.Aircrafts.AnyAsync(a => a.Name.ToLower() == model.Name.Trim().ToLower() && a.Id != model.Id);
        if (nameExists)
        {
            throw new Exception($"An aircraft named '{model.Name}' already exists in the fleet.");
        }

        int seatTotal = model.EconomySeats + model.BusinessSeats + model.FirstClassSeats;
        if (seatTotal > 0)
        {
            model.Capacity = seatTotal;
        }
        else if (model.Capacity > 0)
        {
            model.EconomySeats = model.Capacity;
        }

        aircraft.Name = model.Name.Trim();
        aircraft.Capacity = model.Capacity;
        aircraft.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();
        aircraft.Status = model.Status;

        // Check if seat configuration changed
        int currentEconomy = aircraft.Seats?.Count(s => s.Class == SeatClass.Economy) ?? 0;
        int currentBusiness = aircraft.Seats?.Count(s => s.Class == SeatClass.Business) ?? 0;
        int currentFirst = aircraft.Seats?.Count(s => s.Class == SeatClass.FirstClass) ?? 0;

        bool seatsChanged = currentEconomy != model.EconomySeats ||
                            currentBusiness != model.BusinessSeats ||
                            currentFirst != model.FirstClassSeats ||
                            aircraft.Seats == null ||
                            !aircraft.Seats.Any();

        if (seatsChanged)
        {
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.Seat.AircraftId == model.Id);
            if (hasBookings)
            {
                throw new Exception("Cannot modify cabin seat distribution because active passenger bookings exist for this aircraft.");
            }

            if (aircraft.Seats != null && aircraft.Seats.Any())
            {
                _context.Seats.RemoveRange(aircraft.Seats);
                await _context.SaveChangesAsync();
            }

            var newSeats = GenerateSeatsForAircraft(model.FirstClassSeats, model.BusinessSeats, model.EconomySeats, model.Capacity);
            foreach (var seat in newSeats)
            {
                seat.AircraftId = aircraft.Id;
            }
            await _context.Seats.AddRangeAsync(newSeats);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        bool hasFlights = await _context.Flights.AnyAsync(a => a.AircraftId == id);
        if (hasFlights)
        {
            throw new Exception("Aircraft is used in Flights");
        }
        var aircraft = await _context.Aircrafts.FindAsync(id);
        if (aircraft == null)
        {
            throw new Exception("Aircraft Not Found");
        }
        _context.Aircrafts.Remove(aircraft);
        await _context.SaveChangesAsync();
    }

    private List<Seat> GenerateSeatsForAircraft(int firstClassCount, int businessCount, int economyCount, int totalCapacity)
    {
        var seats = new List<Seat>();
        int currentRow = 1;

        // 1. First Class Seats (typically 4 across: A, B, C, D)
        if (firstClassCount > 0)
        {
            char[] firstLetters = new[] { 'A', 'B', 'C', 'D' };
            int created = 0;
            while (created < firstClassCount)
            {
                foreach (var letter in firstLetters)
                {
                    if (created >= firstClassCount) break;
                    seats.Add(new Seat
                    {
                        SeatNumber = $"{currentRow}{letter}",
                        Class = SeatClass.FirstClass
                    });
                    created++;
                }
                currentRow++;
            }
        }

        // 2. Business Class Seats (typically 6 across: A, B, C, D, E, F)
        if (businessCount > 0)
        {
            char[] busLetters = new[] { 'A', 'B', 'C', 'D', 'E', 'F' };
            int created = 0;
            while (created < businessCount)
            {
                foreach (var letter in busLetters)
                {
                    if (created >= businessCount) break;
                    seats.Add(new Seat
                    {
                        SeatNumber = $"{currentRow}{letter}",
                        Class = SeatClass.Business
                    });
                    created++;
                }
                currentRow++;
            }
        }

        // 3. Economy Class Seats (6 across: A, B, C, D, E, F)
        if (economyCount > 0)
        {
            char[] econLetters = new[] { 'A', 'B', 'C', 'D', 'E', 'F' };
            int created = 0;
            while (created < economyCount)
            {
                foreach (var letter in econLetters)
                {
                    if (created >= economyCount) break;
                    seats.Add(new Seat
                    {
                        SeatNumber = $"{currentRow}{letter}",
                        Class = SeatClass.Economy
                    });
                    created++;
                }
                currentRow++;
            }
        }
        else if (firstClassCount == 0 && businessCount == 0 && totalCapacity > 0)
        {
            // Default fallback if no cabin breakdown was specified: populate as Economy seats
            char[] econLetters = new[] { 'A', 'B', 'C', 'D', 'E', 'F' };
            int created = 0;
            while (created < totalCapacity)
            {
                foreach (var letter in econLetters)
                {
                    if (created >= totalCapacity) break;
                    seats.Add(new Seat
                    {
                        SeatNumber = $"{currentRow}{letter}",
                        Class = SeatClass.Economy
                    });
                    created++;
                }
                currentRow++;
            }
        }

        return seats;
    }
}
