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
                ImageUrl = a.ImageUrl
            })
            .ToListAsync();
    }

    public async Task<AircraftVM> GetAircraftByIdAsync(int id)
    {
        var aircraft = await _context.Aircrafts
            .Where(a => a.Id == id)
            .Select(a => new AircraftVM
            {
                Id = a.Id,
                Name = a.Name,
                Capacity = a.Capacity,
                ImageUrl = a.ImageUrl
            })
            .FirstOrDefaultAsync();

        if (aircraft == null)
        {
            throw new Exception("Aircraft Not Found");
        }

        return aircraft;
    }

    public async Task CreateAircraftAsync(AircraftVM model)
    {
        bool aircraftExists = await _context.Aircrafts.AnyAsync(a => a.Name == model.Name);
        if (aircraftExists)
        {
            throw new Exception("Aircraft with this name already exists");
        }

        var aircraft = new Aircraft
        {
            Name = model.Name,
            Capacity = model.Capacity,
            ImageUrl = model.ImageUrl
        };

        _context.Aircrafts.Add(aircraft);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAircraftAsync(AircraftVM model)
    {
        var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == model.Id);
        if (aircraft == null)
        {
            throw new Exception("Aircraft Not Found");
        }

        aircraft.Name = model.Name;
        aircraft.Capacity = model.Capacity;
        aircraft.ImageUrl = model.ImageUrl;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAircraftAsync(int id)
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
}