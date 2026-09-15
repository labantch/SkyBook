using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class BookingService : IBookingService
{
    private ApplicationDbContext context = new ApplicationDbContext();

    public async Task<List<MyBookingVM>> GetUserBookingsAsync()
    {
        var book = await context.Bookings
            .Include(b => b.Flight).ThenInclude(flight => flight.ArrivalAirport)
            .Include(b => b.Seat)
            .ToListAsync();
        var list = new List<MyBookingVM>();
        foreach (var b in book)
        {
            var vm = new MyBookingVM()
            {
                BookingId = b.Id,
                BookingReference = b.BookingReference,
                FlightNumber = b.Flight.FlightNumber,
                ArrivalAirPort = b.Flight.ArrivalAirport.Name,
                DepartureTime = b.Flight.DepartureTime,
                SeatNumber = b.Seat.SeatNumber,
                bookingStuatus = b.Status
            };
            list.Add(vm);
        }
        return list;
    }

    // public async Task<List<MyBookingVM>> GetAllBookingAsync()
    // {
    //     var book = await context.Bookings.ToListAsync();
    //     var list = new List<Booking>();
    //     foreach (var b in book)
    //     {
    //         var vm = new Booking()
    //         {
    //             Id = b.Id,
    //             BookingReference = b.BookingReference,
    //             BookingDate = b.BookingDate,
    //
    //         };
    //         list.Add(vm);
    //     }
    //     return list;
    // }

    public async Task<BookingDetailsVm> GetBookingByIdAsync(int id)
    {
        var books = await context.Bookings
            .Include(booking => booking.Flight).ThenInclude(flight => flight.ArrivalAirport)
            .Include(booking => booking.Flight).ThenInclude(flight => flight.DepartureAirport)
            .Include(booking => booking.Passenger)
            .Include(booking => booking.Seat).Include(booking => booking.User)
            .FirstOrDefaultAsync(b => b.Id == id);
        var book = new BookingDetailsVm()
        {
           BookingId = books.Id,
           BookingDate = books.BookingDate,
           TotalPrice = books.TotalPrice,
           Status = books.Status,
           BookingReference = books.BookingReference,
           DepartureAirPort = books.Flight.DepartureAirport.Name,
           ArrivalAirPort = books.Flight.ArrivalAirport.Name,
           PassengerName = books.User.FullName,
           FlightNumber = books.Flight.FlightNumber,
           SeatNumber = books.Seat.SeatNumber
        };
        return book;
    }

    public async Task CancelAsync(int id)
    {
        var book = await context.Bookings.FirstOrDefaultAsync(s => s.Id == id);
        if (book != null)
        {
            context.Bookings.Remove(book);
            await context.SaveChangesAsync();
        }
    }

    public async Task<CreateBookingVM> AddAsync(int id)
    {
        var books = await context.Bookings
            .Include(booking => booking.Passenger)
            .Include(booking => booking.User)
            .FirstOrDefaultAsync(b => b.Id == id);
        var book=new CreateBookingVM()
        {
            FlightId = books.FlightId,
            SeatID = books.SeatId,
            Email = books.User.Email,
            FullName = books.User.FullName,
            PassportNumber = books.Passenger.PassportNumber,
            PhoneNumber = books.User.PhoneNumber
           
        };
        
        await context.Bookings.AddAsync(books);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> IsSeatBookedAsync(int id)
    {
        var seat = await context.Seats.FirstOrDefaultAsync(s => s.Id == id);
        if (seat == null)
        {
            return false;
        }

        return true;
    }
}