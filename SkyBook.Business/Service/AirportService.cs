using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class AirportService : IAirportService
{
    private readonly ApplicationDbContext _context;

    public AirportService(ApplicationDbContext context)
    {
        _context = context;
    }
    #region  GetAllAirport
    public async Task<List<AirportVM>> GetAllAirportAsync()
    {
        return await _context.Airports
            .Select(a => new AirportVM
            {
                Id = a.Id,
                Name = a.Name,
                Code = a.Code,
                City = a.City,
                Country = a.Country,
                ImageUrl = a.ImageUrl,
                Status = a.Status
            })
            .ToListAsync();
    }
    #endregion

    #region GetAirportById
    public async Task<AirportVM> GetAirportByIdAsync(int id)
    {
        var airport = await _context.Airports
            .Where(a => a.Id == id)
            .Select(a => new AirportVM
            {
                Id = a.Id,
                Name = a.Name,
                Code = a.Code,
                City = a.City,
                Country = a.Country,
                ImageUrl = a.ImageUrl,
                Status = a.Status
            })
            .FirstOrDefaultAsync();

        if (airport == null)
        {
            throw new Exception("Airport Not Found");
        }

        return airport;
    }
    #endregion

    #region Create
    public async Task CreateAsync(AirportVM model)
    {
        bool codeExists = await _context.Airports.AnyAsync(a => a.Code == model.Code);
        if (codeExists)
        {
            throw new Exception("Airport Code Already Exists");
        }

        var airport = new Airport
        {
            Name = model.Name,
            Code = model.Code,
            City = model.City,
            Country = model.Country,
            ImageUrl = model.ImageUrl,
            Status = model.Status
        };

        _context.Airports.Add(airport);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region  UpdateAirport
    public async Task UpdateAirportAsync(AirportVM model)
    {
        var airport = await _context.Airports.FirstOrDefaultAsync(a => a.Id == model.Id);
        if (airport == null)
        {
            throw new Exception("Airport Not Found");
        }

        airport.Name = model.Name;
        airport.Country = model.Country;
        airport.Code = model.Code;
        airport.City = model.City;
        airport.ImageUrl = model.ImageUrl;
        airport.Status = model.Status;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region DeleteAirport
    public async Task DeleteAirportAsync(int id)
    {
        bool hasFlights = await _context.Flights.AnyAsync(f => f.DepartureAirportId == id || f.ArrivalAirportId == id);
        if (hasFlights)
        {
            throw new Exception("Cannot delete airport because it is used in flights");
        }

        var airport = await _context.Airports.FindAsync(id);
        if (airport == null)
        {
            throw new Exception("Airport Not Found");
        }

        _context.Airports.Remove(airport);
        await _context.SaveChangesAsync();
    }
    #endregion
}
