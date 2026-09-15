using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class PassengerService : IPassengerService
{
    private ApplicationDbContext context = new ApplicationDbContext();

    public async Task<List<Passenger>> GetAll()
    {
        var passenger = await context.Passengers.ToListAsync();
        var list = new List<Passenger>();
        foreach (var p in passenger)
        {
            var vm = new Passenger()
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Nationality = p.Nationality
            };
            list.Add(vm);
        }
        return list;
    }

    public async Task<List<Passenger>> GetById(int id)
    {
        var passenger = await context.Passengers.ToListAsync();
        var list = new List<Passenger>();
        foreach (var p in passenger)
        {
            var vm = new Passenger()
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Nationality = p.Nationality,
                DateOfBirth = p.DateOfBirth,
                PassportNumber = p.PassportNumber
                
            };
            list.Add(vm);
        }
        return list;
    }

    public async Task Add(Passenger passenger)
    {
        await context.Passengers.AddAsync(passenger);
        await context.SaveChangesAsync();
    }

    public async Task Update(Passenger passenger)
    {
        context.Passengers.Update(passenger);
        await context.SaveChangesAsync();

    }

    public async Task Delete(int id)
    {
        var airport = await context.Airports.FirstOrDefaultAsync(a => a.Id == id);
        if (airport != null)
        {
            context.Airports.Remove(airport);
            await context.SaveChangesAsync();
        }
    }
}