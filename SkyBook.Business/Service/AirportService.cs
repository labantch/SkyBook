using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class AirportService: IAirportService
{
   private readonly ApplicationDbContext _context;
    public AirportService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<AirportVM>> GetAllAirportAsync()
    {
        return await _context.Airports
            .Select(a=> new AirportVM { id=a.Id,Name=a.Name,
             Code=a.Code,City=a.City,Country=a.Country})
            .ToListAsync();
    }

    public async Task<AirportVM> GetAirportByIdAsync(int id)
    {
       return await _context.Airports
            .Where(a =>a.Id== id)
            .Select(a =>new AirportVM 
            {Name=a.Name ,id=a.Id,City=a.City,Country=a.Country })
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(AirportVM model)
    {
     bool codeExists=await _context.Airports.AnyAsync(a=>a.Code==model.Code);
        if (codeExists)
        {
            throw new Exception("Airport Code Already Existe");
        }
     var airport = new Airport
     {
            Name = model.Name,
            Code = model.Code,
            City = model.City,
            Country = model.Country
      };
        _context.Airports.Add(airport);
        _context.SaveChangesAsync();

    }

    public async Task UpdateAirportAsync(AirportVM model)
    {
        var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == model.id);
        if (airport == null)
        {
            throw new Exception("Airport Not Found");
        }
        airport.Name = model.Name;
        airport.Country = model.Country;
        airport.Code = model.Code;
        airport.City = model.City;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAirportAsync(int id)
    {
        bool hasFlights = await _context.Flights.AnyAsync(f => f.DepartureAirportId == id || f.ArrivalAirportId == id);
        if (hasFlights)
        {
            throw new Exception("Cannot delete airport becouse it is used in flights");
        }
        var airport = await _context.Airports.FindAsync(id);
        if (airport == null)
        {
            throw new Exception("Airport Not Found");
        }
        _context.Airports.Remove(airport);
        _context.SaveChangesAsync();
    }
}