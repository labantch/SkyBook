using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IBookingService
{
    public Task<List<MyBookingVM>> GetUserBookingsAsync(string userId);
    public Task<BookingDetailsVm> GetBookingByIdAsync (int bookingId,string userId);
    public Task CancelAsync(int bookingId,string userId);
    public Task<int> CreateBookingAsync(string userId,CreateBookingVM model);
    public Task<bool> IsSeatAvailableAsync(int flightId, int SeatId);
    public Task<int> GetAvailableSeatsCountAsync(int flightId);
    public Task<List<BookingDetailsVm>> GetAllBookingsAsync();
    public Task<BookingDetailsVm> GetBookingByIdAsync(int bookingId);
    public Task CancelAsync(int bookingId);
    public Task<BookingStatus> ToggleStatusAsync(int bookingId);
    public Task UncancelAsync(int bookingId);
    public Task<object> ConfirmBookingAsync(string? userId, ConfirmBookingDto model);
    public Task<List<string>> GetOccupiedSeatNumbersAsync(int? flightId, string? flightNumber);
    public Task<FlightSeatLayoutDto?> GetFlightSeatLayoutAsync(int? flightId, string? flightNumber);
}