using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class AircraftService: IAircraftService
{
    private readonly ApplicationDbContext context;

    public AircraftService(ApplicationDbContext _context)
    {
        context = _context;
    }
    
    public void Add(Aircraft aircraft)
    {
        context.Aircrafts.Add(aircraft);
        context.SaveChanges();
    }

    

    public void Delete(int id)
    {
        var aircraft = context.Aircrafts.FirstOrDefault(d => d.Id == id);
        if (aircraft != null)
        {
            context.Aircrafts.Remove(aircraft);
            context.SaveChanges();
        }
    }

    public void Edit(Aircraft aircraft)
    {
        context.Aircrafts.Update(aircraft);
        context.SaveChanges();
    }
}