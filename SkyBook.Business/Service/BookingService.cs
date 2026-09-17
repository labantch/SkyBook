using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;
using System.Net.NetworkInformation;

namespace SkyBook.Business.Service;

public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }
    #region CreateBooking
    public async Task CreateBookingAsync( string userId, CreateBookingVM model)
        {
    
            var flight = await _context.Flights
                .FirstOrDefaultAsync(f => f.Id == model.FlightId);
            if (flight == null)
                throw new Exception("Flight not found.");
            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.Id == model.SeatId && s.AircraftId == flight.AircraftId);
            if (seat == null)
                throw new Exception("Seat not found for this aircraft.");
            var isBooked = await IsSeatAvailableAsync( model.FlightId, model.SeatId);
            if (!isBooked)
                throw new Exception("This seat is already booked.");
            var passenger = new Passenger
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Nationality = model.Nationality,
                PassportNumber = model.PassportNumber,
                Email = model.Email,
                Phone = model.PhoneNumber
            };

            _context.Passengers.Add(passenger);
            var bookingReference = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper();
            var booking = new Booking
            {
                UserId = userId,
                Passenger = passenger,
                FlightId = model.FlightId,
                SeatId = model.SeatId,
                BookingDate = DateTime.Now,
                TotalPrice = flight.Price,
                BookingReference = bookingReference,
                Status = BookingStatus.Confirmed
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }
    #endregion

    #region GetUserBookings
    public async Task<List<MyBookingVM>> GetUserBookingsAsync(
            string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Flight)
                .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Seat)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var result = bookings.Select(b => new MyBookingVM
            {
                BookingId = b.Id,
                BookingReference = b.BookingReference,
                FlightNumber = b.Flight.FlightNumber,
                DepartureAirPort =  b.Flight.DepartureAirport.Name,
                ArrivalAirPort = b.Flight.ArrivalAirport.Name,
                DepartureTime = b.Flight.DepartureTime,
                SeatNumber =   b.Seat.SeatNumber,
                Status =  b.Status

            }).ToList();
            return result;
        }
    #endregion
    
    #region GetBookingById
    public async Task<BookingDetailsVm> GetBookingByIdAsync(
            int bookingId,
            string userId)
    {
        var booking = await _context.Bookings

            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .Include(b => b.Flight)
            .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
            .ThenInclude(f => f.ArrivalAirport)
            .FirstOrDefaultAsync(b =>  b.Id == bookingId &&b.UserId == userId);
        if (booking == null)
            throw new Exception("Booking not found.");
        var result = new BookingDetailsVm
        {
            BookingId = booking.Id,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingReference = booking.BookingReference,
            DepartureAirPort = booking.Flight.DepartureAirport.Name,
            ArrivalAirPort =booking.Flight.ArrivalAirport.Name,
            PassengerName =$"{booking.Passenger.FirstName} {booking.Passenger.LastName}",
            FlightNumber = booking.Flight.FlightNumber,
            SeatNumber =booking.Seat.SeatNumber
        };
        return result;
    }
    #endregion
    
    #region Cancel

    public async Task CancelAsync( int bookingId, string userId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b =>b.Id == bookingId &&b.UserId == userId);
        if (booking == null)
            throw new Exception("Booking not found.");
        if (booking.Status == BookingStatus.Cancelled)
            throw new Exception("Booking is already cancelled.");
        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
    }
    #endregion
    
    #region IsSeatAvailable
    public async Task<bool> IsSeatAvailableAsync( int flightId, int seatId)
    {
        var isBooked = await _context.Bookings
        .AnyAsync(b =>
                b.FlightId == flightId &&
                b.SeatId == seatId &&
                b.Status != BookingStatus.Cancelled);
        return !isBooked;
    }
    #endregion
    
    #region GetAvailableSeatsCount
    public async Task<int> GetAvailableSeatsCountAsync(int flightId)
    {
        
        var flight = await _context.Flights
            .Include(f => f.Aircraft)
            .FirstOrDefaultAsync(f => f.Id == flightId);
        if (flight == null)
            throw new Exception("Flight not found.");        
        var totalSeats = await _context.Seats.CountAsync(s =>s.AircraftId == flight.AircraftId);
        var bookedSeats = await _context.Bookings.CountAsync(b =>
                    b.FlightId == flightId &&
                    b.Status != BookingStatus.Cancelled);
        return totalSeats - bookedSeats;
    }
    #endregion

    public async Task<List<BookingDetailsVm>> GetBookingByIdAsync(string userId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Flight)
            .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
            .ThenInclude(f => f.ArrivalAirport)
            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .Where(b => b.UserId == userId)
            .ToListAsync();
        return bookings.Select(booking => new BookingDetailsVm
        {
            BookingId = booking.Id,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingReference = booking.BookingReference,
            DepartureAirPort = booking.Flight.DepartureAirport.Name,
            ArrivalAirPort = booking.Flight.ArrivalAirport.Name,
            PassengerName = $"{booking.Passenger.FirstName} {booking.Passenger.LastName}",
            FlightNumber = booking.Flight.FlightNumber,
            SeatNumber = booking.Seat.SeatNumber,
            UserImageUrl = booking.User?.ImageUrl
        }).ToList();
    }

    public async Task<List<BookingDetailsVm>> GetAllBookingsAsync()
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Flight)
                .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
                .ThenInclude(f => f.ArrivalAirport)
            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return bookings.Select(booking => new BookingDetailsVm
        {
            BookingId = booking.Id,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingReference = booking.BookingReference,
            DepartureAirPort = booking.Flight?.DepartureAirport?.Name ?? "",
            ArrivalAirPort = booking.Flight?.ArrivalAirport?.Name ?? "",
            PassengerName = booking.Passenger != null ? $"{booking.Passenger.FirstName} {booking.Passenger.LastName}" : "Unknown Passenger",
            FlightNumber = booking.Flight?.FlightNumber ?? "",
            SeatNumber = booking.Seat?.SeatNumber ?? "",
            UserImageUrl = booking.User?.ImageUrl
        }).ToList();
    }

    public async Task<List<BookingDetailsVm>> ListAllBookingsAsync()
    {
        return await GetAllBookingsAsync();
    }

    public async Task<BookingDetailsVm> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .Include(b => b.Flight)
                .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
                .ThenInclude(f => f.ArrivalAirport)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new Exception("Booking not found.");

        return new BookingDetailsVm
        {
            BookingId = booking.Id,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingReference = booking.BookingReference,
            DepartureAirPort = booking.Flight?.DepartureAirport?.Name ?? "",
            ArrivalAirPort = booking.Flight?.ArrivalAirport?.Name ?? "",
            PassengerName = booking.Passenger != null ? $"{booking.Passenger.FirstName} {booking.Passenger.LastName}" : "Unknown Passenger",
            FlightNumber = booking.Flight?.FlightNumber ?? "",
            SeatNumber = booking.Seat?.SeatNumber ?? "",
            UserImageUrl = booking.User?.ImageUrl
        };
    }

    public async Task CancelAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");
        if (booking.Status == BookingStatus.Cancelled)
            throw new Exception("Booking is already cancelled.");
        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    public async Task<BookingStatus> ToggleStatusAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");

        if (booking.Status == BookingStatus.Cancelled)
        {
            booking.Status = BookingStatus.Confirmed;
        }
        else
        {
            booking.Status = BookingStatus.Cancelled;
        }

        await _context.SaveChangesAsync();
        return booking.Status;
    }

    public async Task UncancelAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");

        booking.Status = BookingStatus.Confirmed;
        await _context.SaveChangesAsync();
    }
}


