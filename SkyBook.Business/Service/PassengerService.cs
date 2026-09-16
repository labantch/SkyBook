using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class PassengerService : IPassengerService
{
    private ApplicationDbContext context = new ApplicationDbContext();

    public PassengerService(ApplicationDbContext context)
    {
        this.context = context;
    }
    
    public async Task<List<PassengerVM>> GetAllPassengersAsync()
    {
        return await context.Passengers
            .Select(p => new PassengerVM
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
            })
            .ToListAsync();
    }

    public async Task<PassengerVM> GetPassengersByIdAsync(int id)
    {
        return await context.Passengers
            .Where(p => p.Id == id)
            .Select(p => new PassengerVM
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                Nationality = p.Nationality,
                PassportNumber = p.PassportNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task AddPassengerAsync(PassengerVM passenger)
    {
        bool passengerExists = await context.Passengers.AnyAsync(p => p.Id == passenger.Id);
        if (passengerExists)
        {
            throw new Exception("Passenger Already Exist");
        }

        var passenger1 = new Passenger
        {
            FirstName = passenger.FirstName,
            LastName = passenger.LastName,
            DateOfBirth = passenger.DateOfBirth,
            Nationality = passenger.Nationality,
            PassportNumber = passenger.PassportNumber,
            Id = passenger.Id
        };
        await context.Passengers.AddAsync(passenger1);
        await context.SaveChangesAsync();
    }

    public async Task UpdatePassengerAsync(PassengerVM passenger)
    {
        var passenger1 = await context.Passengers.FirstOrDefaultAsync(p => p.Id == passenger.Id);
        if (passenger1 == null)
        {
            throw new Exception("Passenger Not Found");
        }

        passenger1.Id = passenger.Id;
        passenger1.FirstName = passenger.FirstName;
        passenger1.LastName = passenger.LastName;
        passenger1.DateOfBirth = passenger.DateOfBirth;
        passenger1.Nationality = passenger.Nationality;
        passenger1.PassportNumber = passenger.PassportNumber;
        await context.SaveChangesAsync();

    }

    public async Task DeletePassengerAsync(int id)
    {
        bool passengerInFlight = await context.Passengers.AnyAsync(p => p.Id == id);
        if (passengerInFlight)
        {
            throw new Exception("Passenger is in a Flight");
        }

        var passenger = await context.Passengers.FindAsync(id);
        if (passenger == null)
        {
            throw new Exception("Passenger Not Found");
        }

        context.Passengers.Remove(passenger);
        await context.SaveChangesAsync();
    }
}