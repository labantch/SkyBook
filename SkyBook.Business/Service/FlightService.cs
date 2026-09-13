using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class FlightService : IFlightService
{
    private ApplicationDbContext context = new ApplicationDbContext();
    public void Creat(Flight flight)
    {
        context.Flights.Add(flight);
        context.SaveChanges();
    }

    public void Edit(Flight flight)
    {
        context.Flights.Update(flight);
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        var flight = context.Flights.FirstOrDefault(a => a.Id == id);
        if (flight != null)
        {
            context.Flights.Remove(flight);
            context.SaveChanges();
        }
    }
}