using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;

namespace SkyBook.Business.Service;

public class SeatService : ISeatService
{
    private readonly ApplicationDbContext _context;
    public SeatService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<SeatSelectionVM>> GetSeatsByFlightAsync(int flightid)
    {
        var aircrftid = await _context.Flights.Where(f => f.Id == flightid).Select(f => f.AircraftId).FirstOrDefaultAsync();
        var seats=await _context.Seats.Where(s=>s.AircraftId==aircrftid)
            .Select(s=> new SeatSelectionVM { SeatId = s.Id,SeatNumber = s.SeatNumber ,SeatClass=s.Class,IsBooked=_context.Bookings
            .Any(b=>b.FlightId==flightid&&b.SeatId==s.Id)}).ToListAsync();
        return seats;
    }


    public async Task<bool> IsSeatAvailableAsync(int seatid, int flightid)
    {
        return !await _context.Bookings.AnyAsync(b => b.SeatId == seatid && b.FlightId == flightid);
    }
}