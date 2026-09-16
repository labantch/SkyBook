using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IPassengerService
{
    public Task<List<PassengerVM>> GetAllPassengersAsync();
    public Task<PassengerVM> GetPassengerByIdAsync(int id);
    //public Task AddPassengerAsync(PassengerVM passenger);
    //public Task UpdatePassengerAsync(PassengerVM passenger);
    //public Task DeletePassengerAsync(int id);
}