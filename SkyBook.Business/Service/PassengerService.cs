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
    #region GetAllPassengers
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
    #endregion
    #region  GetPassengerById

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
    #endregion

    
}