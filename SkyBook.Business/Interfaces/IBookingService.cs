using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IBookingService
{
    public Task<List<MyBookingVM>> GetUserBookingsAsync();
    // public Task<List<MyBookingVM>> GetAllBookingAsync();
    public Task<BookingDetailsVm> GetBookingByIdAsync (int id);
    public Task CancelAsync(int id);
    public Task<CreateBookingVM> AddAsync(int id);
    public Task<bool> IsSeatBookedAsync(int id);
    
}