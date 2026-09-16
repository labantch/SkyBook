using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class PassengerService : IPassengerService
{
    private ApplicationDbContext _context ;

    public PassengerService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<PassengerVM>> GetAllPassengersAsync()
    {
        return await _context.Passengers
            .Select(p => new PassengerVM
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                PassportNumber=p.PassportNumber,
                Nationality=p.Nationality,
                Email=p.Email,
                Phone=p.Phone,
            })
            .ToListAsync();
    }

    public async Task<PassengerVM> GetPassengerByIdAsync(int id)
    {

        var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.Id == id);
        if (passenger == null)
        {
            throw new Exception("Passenger not Found");
        }
        return new PassengerVM
        {
            Id = passenger.Id,
            FirstName = passenger.FirstName,
            LastName = passenger.LastName,
            Nationality = passenger.Nationality,
            PassportNumber = passenger.PassportNumber,
            Email = passenger.Email,
            Phone = passenger.Phone,
            DateOfBirth = passenger.DateOfBirth,
        };
      }

    //public async Task AddPassengerAsync(PassengerVM passenger)
    //{
    //    bool passengerExists = await _context.Passengers.AnyAsync(p => p.Id == passenger.Id);
    //    if (passengerExists)
    //    {
    //        throw new Exception("Passenger Already Exist");
    //    }

    //    var passenger1 = new Passenger
    //    {
    //        FirstName = passenger.FirstName,
    //        LastName = passenger.LastName,
    //        DateOfBirth = passenger.DateOfBirth,
    //        Nationality = passenger.Nationality,
    //        PassportNumber = passenger.PassportNumber,
    //        Id = passenger.Id
    //    };
    //    await _context.Passengers.AddAsync(passenger1);
    //    await _context.SaveChangesAsync();
    //}

    //public async Task UpdatePassengerAsync(PassengerVM passenger)
    //{
    //    var passenger1 = await _context.Passengers.FirstOrDefaultAsync(p => p.Id == passenger.Id);
    //    if (passenger1 == null)
    //    {
    //        throw new Exception("Passenger Not Found");
    //    }

    //    passenger1.Id = passenger.Id;
    //    passenger1.FirstName = passenger.FirstName;
    //    passenger1.LastName = passenger.LastName;
    //    passenger1.DateOfBirth = passenger.DateOfBirth;
    //    passenger1.Nationality = passenger.Nationality;
    //    passenger1.PassportNumber = passenger.PassportNumber;
    //    await _context.SaveChangesAsync();

    //}

    //public async Task DeletePassengerAsync(int id)
    //{
    //    bool passengerInFlight = await _context.Passengers.AnyAsync(p => p.Id == id);
    //    if (passengerInFlight)
    //    {
    //        throw new Exception("Passenger is in a Flight");
    //    }

    //    var passenger = await _context.Passengers.FindAsync(id);
    //    if (passenger == null)
    //    {
    //        throw new Exception("Passenger Not Found");
    //    }

    //    _context.Passengers.Remove(passenger);
    //    await _context.SaveChangesAsync();
    //}
}