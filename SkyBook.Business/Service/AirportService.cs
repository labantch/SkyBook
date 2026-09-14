using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class AirportService: IAirportService
{
    private ApplicationDbContext context = new ApplicationDbContext();
    
    public void Add(Airport airport)
    {
        context.Airports.Add(airport);
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        var airport = context.Airports.FirstOrDefault(a => a.Id == id);
        if (airport != null)
        {
            context.Airports.Remove(airport);
            context.SaveChanges();
        }
    }

    public void Edit(Airport airport)
    {
        context.Airports.Update(airport);
        context.SaveChanges();
    }
}