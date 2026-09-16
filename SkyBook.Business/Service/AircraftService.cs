
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class AircraftService: IAircraftService
{
    private readonly ApplicationDbContext _context;
    public AircraftService(ApplicationDbContext context)
    {
        _context = context;
    }
    #region GetAllAircraft
    public async Task<List<AircraftVM>> GetAllAircraftsAsync()
    {
        return await _context.Aircrafts
            .Select(a=> new AircraftVM 
            { ID=a.Id,Name=a.Name,
              Capecity=a.Capacity,
              imageUrl=a.ImageUrl})
            .ToListAsync();

    }
    #endregion

    #region GetAircraftById
    public async Task<AircraftVM> GetAircraftByIdAsync(int id)
    {
        return await _context.Aircrafts
            .Where(a => a.Id == id)
            .Select(a => new AircraftVM 
            { Capecity = a.Capacity,
              Name = a.Name, 
              imageUrl = a.ImageUrl,ID=a.Id})
            .FirstOrDefaultAsync();
        
    }
    #endregion
    
    #region CreateAircraft
    public async Task CreateAircraftAsync(AircraftVM model)
    {
        bool AircraftExist = await _context.Aircrafts.AnyAsync(a => a.Name == model.Name);
        if (AircraftExist)
        {
            throw new Exception("Aircraft Name Already Exist");
        }
        var aircraft=new Aircraft
        {
            Name=model.Name,
            Capacity=model.Capecity,
            ImageUrl=model.imageUrl
           
        };
          _context.Aircrafts.Add(aircraft);
         await _context.SaveChangesAsync();

    }
    #endregion
    
    #region UpdateAircraft
    public async Task UpdateAircraftAsync(AircraftVM model)
    {
        var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Id == model.ID);
        if (aircraft == null)
        {
            throw new Exception("Aircraft  Not Found");
        }
        var nameExit = await _context.Aircrafts.AnyAsync(a => a.Name == model.Name && a.Id != model.Id);
        if (nameExit)
        {
            throw new Exception("Aircraft Name Already Exist");
            aircraft.Name = model.Name;
            aircraft.Capacity = model.Capecity;
            aircraft.ImageUrl = model.imageUrl;
            await _context.SaveChangesAsync();

        }
    }
    #endregion
    
    #region DeleteAircraft
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
            throw new Exception("Aircraft Not  Found");
        }
        _context.Aircrafts.Remove(aircraft);
        await _context.SaveChangesAsync();
    }
    #endregion




}
