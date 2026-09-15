using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IPassengerService
{
    public Task<List<Passenger>> GetAll();
    public Task<List<Passenger>> GetById(int id);
    public Task Add(Passenger passenger);
    public Task Update(Passenger passenger);
    public Task Delete(int id);
}