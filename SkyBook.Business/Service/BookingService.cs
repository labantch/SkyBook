using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service;

public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }
    #region CreateBooking
    public async Task<int> CreateBookingAsync( string userId, CreateBookingVM model)
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

            decimal seatPrice = seat.Class switch
            {
                SeatClass.FirstClass => flight.FirstClassPrice > 0 ? flight.FirstClassPrice : (flight.Price > 0 ? flight.Price * 4 : 0),
                SeatClass.Business => flight.BusinessPrice > 0 ? flight.BusinessPrice : (flight.Price > 0 ? flight.Price * 2.5m : 0),
                _ => flight.EconomyPrice > 0 ? flight.EconomyPrice : flight.Price
            };

            var booking = new Booking
            {
                UserId = userId,
                Passenger = passenger,
                FlightId = model.FlightId,
                SeatId = model.SeatId,
                BookingDate = DateTime.Now,
                TotalPrice = seatPrice,
                BookingReference = bookingReference,
                Status = BookingStatus.PendingPayment
            };
       
        _context.Bookings.Add(booking);
         await _context.SaveChangesAsync();
        return booking.Id;
        }
    #endregion

    #region GetUserBookings
    public async Task<List<MyBookingVM>> GetUserBookingsAsync(string userId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Passenger)
            .Include(b => b.Flight)
                .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
                .ThenInclude(f => f.ArrivalAirport)
            .Include(b => b.Seat)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        var groups = bookings.GroupBy(b => new
        {
            BaseRef = !string.IsNullOrWhiteSpace(ExtractBaseRef(b.BookingReference)) 
                ? ExtractBaseRef(b.BookingReference) 
                : b.BookingReference,
            b.FlightId
        });

        var result = new List<MyBookingVM>();

        foreach (var g in groups)
        {
            var first = g.First();
            var user = first.User;
            var userName = user?.UserName ?? "";
            var userFullName = !string.IsNullOrWhiteSpace(user?.FullName) ? user.FullName : (user?.UserName ?? "Valued Customer");
            var userImageUrl = user?.ImageUrl;

            var passengers = g.Select(b => new PassengerBookingVm
            {
                PassengerId = b.PassengerId,
                FirstName = b.Passenger?.FirstName ?? "",
                LastName = b.Passenger?.LastName ?? "",
                PassportNumber = b.Passenger?.PassportNumber ?? "",
                Nationality = b.Passenger?.Nationality ?? "",
                SeatNumber = b.Seat?.SeatNumber ?? "",
                TicketReference = b.BookingReference ?? "",
                Price = b.TotalPrice
            }).ToList();

            var combinedSeats = string.Join(", ", g.Select(b => b.Seat?.SeatNumber).Where(s => !string.IsNullOrEmpty(s)).Distinct());
            var status = g.Any(b => b.Status == BookingStatus.Confirmed) ? BookingStatus.Confirmed : first.Status;

            result.Add(new MyBookingVM
            {
                BookingId = first.Id,
                BookingReference = g.Key.BaseRef,
                FlightNumber = first.Flight?.FlightNumber ?? "",
                DepartureAirPort = first.Flight?.DepartureAirport?.Name ?? "",
                ArrivalAirPort = first.Flight?.ArrivalAirport?.Name ?? "",
                DepartureTime = first.Flight?.DepartureTime ?? DateTime.Now,
                ArrivalTime = first.Flight?.ArrivalTime ?? DateTime.Now,
                SeatNumber = !string.IsNullOrEmpty(combinedSeats) ? combinedSeats : (first.Seat?.SeatNumber ?? ""),
                Status = status,
                TotalPrice = g.Sum(b => b.TotalPrice),
                PassengerName = passengers.FirstOrDefault()?.FullName ?? "",
                BookingDate = first.BookingDate,
                UserName = userName,
                UserFullName = userFullName,
                UserImageUrl = userImageUrl,
                Passengers = passengers
            });
        }

        return result;
    }
    #endregion
    
    #region GetBookingById
    public async Task<BookingDetailsVm> GetBookingByIdAsync(int bookingId, string userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .Include(b => b.Flight)
                .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
                .ThenInclude(f => f.ArrivalAirport)
            .FirstOrDefaultAsync(b => b.Id == bookingId && (string.IsNullOrEmpty(userId) || b.UserId == userId));

        if (booking == null)
            throw new Exception("Booking not found.");

        string baseRef = ExtractBaseRef(booking.BookingReference);

        var sisterBookings = await _context.Bookings
            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .Where(b => (b.BookingReference == booking.BookingReference ||
                        (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef))) &&
                        b.FlightId == booking.FlightId &&
                        b.UserId == booking.UserId)
            .ToListAsync();

        var passengers = sisterBookings.Select(b => new PassengerBookingVm
        {
            PassengerId = b.PassengerId,
            FirstName = b.Passenger?.FirstName ?? "",
            LastName = b.Passenger?.LastName ?? "",
            PassportNumber = b.Passenger?.PassportNumber ?? "",
            Nationality = b.Passenger?.Nationality ?? "",
            SeatNumber = b.Seat?.SeatNumber ?? "",
            TicketReference = b.BookingReference ?? "",
            Price = b.TotalPrice
        }).ToList();

        var combinedSeats = string.Join(", ", sisterBookings.Select(b => b.Seat?.SeatNumber).Where(s => !string.IsNullOrEmpty(s)).Distinct());

        return new BookingDetailsVm
        {
            BookingId = booking.Id,
            BookingDate = booking.BookingDate,
            TotalPrice = sisterBookings.Sum(b => b.TotalPrice),
            Status = booking.Status,
            BookingReference = !string.IsNullOrEmpty(baseRef) ? baseRef : booking.BookingReference,
            DepartureAirPort = booking.Flight?.DepartureAirport?.Name ?? "",
            ArrivalAirPort = booking.Flight?.ArrivalAirport?.Name ?? "",
            PassengerName = passengers.FirstOrDefault()?.FullName ?? $"{booking.Passenger?.FirstName} {booking.Passenger?.LastName}",
            FlightNumber = booking.Flight?.FlightNumber ?? "",
            SeatNumber = !string.IsNullOrEmpty(combinedSeats) ? combinedSeats : (booking.Seat?.SeatNumber ?? ""),
            UserImageUrl = booking.User?.ImageUrl,
            UserName = booking.User?.UserName ?? "",
            UserFullName = !string.IsNullOrWhiteSpace(booking.User?.FullName) ? booking.User.FullName : (booking.User?.UserName ?? "Valued Customer"),
            UserEmail = booking.User?.Email ?? "",
            Passengers = passengers
        };
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

        string baseRef = ExtractBaseRef(booking.BookingReference);
        var related = await _context.Bookings
            .Where(b => (b.BookingReference == booking.BookingReference || 
                        (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef))) &&
                        b.FlightId == booking.FlightId &&
                        b.UserId == userId)
            .ToListAsync();

        foreach (var b in related)
        {
            b.Status = BookingStatus.Cancelled;
        }

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

        var groups = bookings.GroupBy(b => new
        {
            BaseRef = !string.IsNullOrWhiteSpace(ExtractBaseRef(b.BookingReference)) 
                ? ExtractBaseRef(b.BookingReference) 
                : b.BookingReference,
            b.UserId,
            b.FlightId
        });

        var result = new List<BookingDetailsVm>();

        foreach (var g in groups)
        {
            var first = g.First();
            var user = first.User;
            var userName = user?.UserName ?? "";
            var userFullName = !string.IsNullOrWhiteSpace(user?.FullName) ? user.FullName : (user?.UserName ?? "Valued Customer");
            var userEmail = user?.Email ?? "";
            var userImageUrl = user?.ImageUrl;

            var passengers = g.Select(b => new PassengerBookingVm
            {
                PassengerId = b.PassengerId,
                FirstName = b.Passenger?.FirstName ?? "",
                LastName = b.Passenger?.LastName ?? "",
                PassportNumber = b.Passenger?.PassportNumber ?? "",
                Nationality = b.Passenger?.Nationality ?? "",
                SeatNumber = b.Seat?.SeatNumber ?? "",
                TicketReference = b.BookingReference ?? "",
                Price = b.TotalPrice
            }).ToList();

            var combinedSeats = string.Join(", ", g.Select(b => b.Seat?.SeatNumber).Where(s => !string.IsNullOrEmpty(s)).Distinct());
            var status = g.Any(b => b.Status == BookingStatus.Confirmed) ? BookingStatus.Confirmed : first.Status;

            result.Add(new BookingDetailsVm
            {
                BookingId = first.Id,
                BookingDate = first.BookingDate,
                TotalPrice = g.Sum(b => b.TotalPrice),
                Status = status,
                BookingReference = g.Key.BaseRef,
                DepartureAirPort = first.Flight?.DepartureAirport?.Name ?? "",
                ArrivalAirPort = first.Flight?.ArrivalAirport?.Name ?? "",
                PassengerName = passengers.FirstOrDefault()?.FullName ?? "Unknown Passenger",
                FlightNumber = first.Flight?.FlightNumber ?? "",
                SeatNumber = !string.IsNullOrEmpty(combinedSeats) ? combinedSeats : (first.Seat?.SeatNumber ?? ""),
                UserImageUrl = userImageUrl,
                UserName = userName,
                UserFullName = userFullName,
                UserEmail = userEmail,
                Passengers = passengers
            });
        }

        return result;
    }

    public async Task<BookingDetailsVm> GetBookingByIdAsync(int bookingId)
    {
        return await GetBookingByIdAsync(bookingId, "");
    }

    public async Task CancelAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");
        await CancelAsync(bookingId, booking.UserId);
    }

    public async Task<BookingStatus> ToggleStatusAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");

        var newStatus = (booking.Status == BookingStatus.Cancelled) ? BookingStatus.Confirmed : BookingStatus.Cancelled;
        string baseRef = ExtractBaseRef(booking.BookingReference);
        var related = await _context.Bookings
            .Where(b => (b.BookingReference == booking.BookingReference || 
                        (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef))) &&
                        b.FlightId == booking.FlightId &&
                        b.UserId == booking.UserId)
            .ToListAsync();

        foreach (var b in related)
        {
            b.Status = newStatus;
        }

        await _context.SaveChangesAsync();
        return newStatus;
    }

    public async Task UncancelAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");

        string baseRef = ExtractBaseRef(booking.BookingReference);
        var related = await _context.Bookings
            .Where(b => (b.BookingReference == booking.BookingReference || 
                        (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef))) &&
                        b.FlightId == booking.FlightId &&
                        b.UserId == booking.UserId)
            .ToListAsync();

        foreach (var b in related)
        {
            b.Status = BookingStatus.Confirmed;
        }

        await _context.SaveChangesAsync();
    }

    private static string ExtractBaseRef(string? refStr)
    {
        if (string.IsNullOrWhiteSpace(refStr)) return "";
        return System.Text.RegularExpressions.Regex.Replace(refStr.Trim(), @"-\d+$", "");
    }

    #region ConfirmBookingAsync
    public async Task<object> ConfirmBookingAsync(string? userId, ConfirmBookingDto model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model), "Invalid booking payload.");

        // Resolve user if not provided
        if (string.IsNullOrEmpty(userId))
        {
            var guestEmail = model.Passengers.FirstOrDefault()?.Email;
            if (!string.IsNullOrEmpty(guestEmail))
            {
                var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == guestEmail);
                if (userByEmail != null) userId = userByEmail.Id;
            }

            if (string.IsNullOrEmpty(userId))
            {
                var defaultUser = await _context.Users.FirstOrDefaultAsync();
                userId = defaultUser?.Id ?? "28FE6275-FA7D-4239-A890-CDDBA0E43395";
            }
        }

        var pnr = !string.IsNullOrWhiteSpace(model.BookingReference)
            ? model.BookingReference.Trim()
            : "SKY-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        if (await _context.Bookings.AnyAsync(b => b.BookingReference == pnr))
        {
            pnr = $"{pnr}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";
        }

        if (model.Passengers == null || model.Passengers.Count == 0)
        {
            model.Passengers = new List<ConfirmPassengerDto>
            {
                new ConfirmPassengerDto { FirstName = "Guest", LastName = "Traveler", Nationality = "Egypt", Passport = "PASS-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper() }
            };
        }

        if (model.Segments == null || model.Segments.Count == 0)
        {
            var fallback = await _context.Flights.Include(f => f.Aircraft).FirstOrDefaultAsync();
            model.Segments = new List<ConfirmFlightSegmentDto>
            {
                new ConfirmFlightSegmentDto { FlightId = fallback?.Id ?? 3, FlightNumber = fallback?.FlightNumber ?? "SKB-101", Price = model.TotalPrice > 0 ? model.TotalPrice : 2560 }
            };
        }

        var createdBookings = new List<Booking>();
        var totalTickets = model.Segments.Count * model.Passengers.Count;
        decimal perTicketPrice = model.TotalPrice > 0 ? Math.Round(model.TotalPrice / totalTickets, 2) : 1200.00m;
        var claimedSeatIdsByFlight = new Dictionary<int, HashSet<int>>();

        for (int pIdx = 0; pIdx < model.Passengers.Count; pIdx++)
        {
            var pDto = model.Passengers[pIdx];
            var passportNum = !string.IsNullOrWhiteSpace(pDto.Passport) 
                ? pDto.Passport.Trim() 
                : ("PASS-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper());

            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == passportNum);
            if (passenger == null)
            {
                DateTime dob = DateTime.TryParse(pDto.Dob, out var parsedDob) ? parsedDob : new DateTime(2000, 1, 1);
                passenger = new Passenger
                {
                    FirstName = !string.IsNullOrWhiteSpace(pDto.FirstName) ? pDto.FirstName.Trim() : $"Passenger {pIdx + 1}",
                    LastName = !string.IsNullOrWhiteSpace(pDto.LastName) ? pDto.LastName.Trim() : "Traveler",
                    DateOfBirth = dob,
                    Nationality = !string.IsNullOrWhiteSpace(pDto.Nationality) ? pDto.Nationality.Trim() : "Egypt",
                    PassportNumber = passportNum,
                    Email = pDto.Email,
                    Phone = pDto.Phone
                };
                _context.Passengers.Add(passenger);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(pDto.FirstName)) passenger.FirstName = pDto.FirstName.Trim();
                if (!string.IsNullOrWhiteSpace(pDto.LastName)) passenger.LastName = pDto.LastName.Trim();
                if (!string.IsNullOrWhiteSpace(pDto.Nationality)) passenger.Nationality = pDto.Nationality.Trim();
                if (!string.IsNullOrWhiteSpace(pDto.Email)) passenger.Email = pDto.Email.Trim();
                if (!string.IsNullOrWhiteSpace(pDto.Phone)) passenger.Phone = pDto.Phone.Trim();
                if (DateTime.TryParse(pDto.Dob, out var updatedDob)) passenger.DateOfBirth = updatedDob;
                await _context.SaveChangesAsync();
            }

            for (int sIdx = 0; sIdx < model.Segments.Count; sIdx++)
            {
                var seg = model.Segments[sIdx];
                var isReturn = sIdx > 0;

                Flight? flight = null;
                if (seg.FlightId.HasValue && seg.FlightId.Value > 0)
                    flight = await _context.Flights.Include(f => f.Aircraft).FirstOrDefaultAsync(f => f.Id == seg.FlightId.Value);

                if (flight == null && !string.IsNullOrWhiteSpace(seg.FlightNumber))
                {
                    var cleanNum = System.Text.RegularExpressions.Regex.Replace(seg.FlightNumber, @"[^\w\-]", "").Trim();
                    flight = await _context.Flights.Include(f => f.Aircraft).FirstOrDefaultAsync(f => 
                        f.FlightNumber == cleanNum || f.FlightNumber.Contains(cleanNum) || cleanNum.Contains(f.FlightNumber));
                }

                if (flight == null)
                    flight = await _context.Flights.Include(f => f.Aircraft).FirstOrDefaultAsync();

                if (flight == null) continue;

                if (!claimedSeatIdsByFlight.TryGetValue(flight.Id, out var claimedSet))
                {
                    var takenFromDb = await _context.Bookings
                        .Where(b => b.FlightId == flight.Id && b.Status != BookingStatus.Cancelled)
                        .Select(b => b.SeatId)
                        .ToListAsync();
                    claimedSet = new HashSet<int>(takenFromDb);
                    claimedSeatIdsByFlight[flight.Id] = claimedSet;
                }

                var seatString = isReturn ? pDto.SeatReturn : pDto.SeatOutbound;
                var seatMatch = !string.IsNullOrEmpty(seatString) ? System.Text.RegularExpressions.Regex.Match(seatString, @"\d+[A-Z]").Value : "";

                Seat? seat = null;
                if (!string.IsNullOrEmpty(seatMatch))
                {
                    seat = await _context.Seats.FirstOrDefaultAsync(s => s.AircraftId == flight.AircraftId && s.SeatNumber == seatMatch);
                }

                // If seat not found, or already occupied by an active booking or another passenger in this batch:
                if (seat == null || claimedSet.Contains(seat.Id))
                {
                    SeatClass desiredClass = seat != null ? seat.Class : (seg.Price > 1500 ? SeatClass.FirstClass : (seg.Price > 600 ? SeatClass.Business : SeatClass.Economy));

                    seat = await _context.Seats
                        .Where(s => s.AircraftId == flight.AircraftId && s.Class == desiredClass && !claimedSet.Contains(s.Id))
                        .OrderBy(s => s.Id)
                        .FirstOrDefaultAsync();

                    if (seat == null)
                    {
                        seat = await _context.Seats
                            .Where(s => s.AircraftId == flight.AircraftId && !claimedSet.Contains(s.Id))
                            .OrderBy(s => s.Id)
                            .FirstOrDefaultAsync();
                    }
                }

                if (seat != null)
                {
                    claimedSet.Add(seat.Id);
                }

                var ticketRef = createdBookings.Count == 0 ? pnr : $"{pnr}-{(createdBookings.Count + 1)}";

                var booking = new Booking
                {
                    UserId = userId,
                    PassengerId = passenger.Id,
                    FlightId = flight.Id,
                    SeatId = seat?.Id ?? 1051,
                    BookingDate = DateTime.Now,
                    TotalPrice = perTicketPrice,
                    BookingReference = ticketRef,
                    Status = BookingStatus.Confirmed
                };

                _context.Bookings.Add(booking);
                createdBookings.Add(booking);
            }
        }

        await _context.SaveChangesAsync();

        return new 
        { 
            success = true, 
            bookingReference = pnr, 
            bookingsCreated = createdBookings.Count,
            message = "Reservation successfully persisted to SQL Server database." 
        };
    }
    #endregion

    #region GetOccupiedSeatNumbersAsync
    public async Task<List<string>> GetOccupiedSeatNumbersAsync(int? flightId, string? flightNumber)
    {
        var query = _context.Bookings.Where(b => b.Status != BookingStatus.Cancelled);

        if (flightId.HasValue && flightId.Value > 0)
        {
            query = query.Where(b => b.FlightId == flightId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(flightNumber))
        {
            var cleanNum = System.Text.RegularExpressions.Regex.Replace(flightNumber, @"[^\w\-]", "").Trim();
            query = query.Where(b => b.Flight.FlightNumber == cleanNum || b.Flight.FlightNumber.Contains(cleanNum) || cleanNum.Contains(b.Flight.FlightNumber));
        }
        else
        {
            return new List<string>();
        }

        return await query
            .Include(b => b.Seat)
            .Where(b => b.Seat != null && !string.IsNullOrEmpty(b.Seat.SeatNumber))
            .Select(b => b.Seat.SeatNumber)
            .Distinct()
            .ToListAsync();
    }
    #endregion

    #region GetFlightSeatLayoutAsync
    public async Task<FlightSeatLayoutDto?> GetFlightSeatLayoutAsync(int? flightId, string? flightNumber)
    {
        Flight? flight = null;
        if (flightId.HasValue && flightId.Value > 0)
        {
            flight = await _context.Flights
                .Include(f => f.Aircraft)
                    .ThenInclude(a => a.Seats)
                .FirstOrDefaultAsync(f => f.Id == flightId.Value);
        }

        if (flight == null && !string.IsNullOrWhiteSpace(flightNumber))
        {
            var cleanNum = System.Text.RegularExpressions.Regex.Replace(flightNumber, @"[^\w\-]", "").Trim();
            flight = await _context.Flights
                .Include(f => f.Aircraft)
                    .ThenInclude(a => a.Seats)
                .FirstOrDefaultAsync(f => f.FlightNumber == cleanNum || f.FlightNumber.Contains(cleanNum) || cleanNum.Contains(f.FlightNumber));
        }

        if (flight == null)
        {
            return null;
        }

        var occupiedSeats = await _context.Bookings
            .Where(b => b.FlightId == flight.Id && b.Status != BookingStatus.Cancelled && b.Seat != null)
            .Select(b => b.Seat.SeatNumber)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToListAsync();

        var aircraft = flight.Aircraft;
        var seats = aircraft?.Seats?.OrderBy(s => s.Id).ToList() ?? new List<Seat>();

        var seatDetails = new List<SeatDetailDto>();
        foreach (var s in seats)
        {
            var match = System.Text.RegularExpressions.Regex.Match(s.SeatNumber ?? "", @"^(\d+)([A-Z])$");
            int row = 0;
            string letter = "";
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out row);
                letter = match.Groups[2].Value;
            }

            string cabin = s.Class switch
            {
                SeatClass.FirstClass => "first",
                SeatClass.Business => "business",
                _ => "economy"
            };

            seatDetails.Add(new SeatDetailDto
            {
                SeatNumber = s.SeatNumber ?? "",
                CabinClass = cabin,
                Row = row,
                Letter = letter
            });
        }

        int firstCount = seats.Count(s => s.Class == SeatClass.FirstClass);
        int busCount = seats.Count(s => s.Class == SeatClass.Business);
        int econCount = seats.Count(s => s.Class == SeatClass.Economy);

        return new FlightSeatLayoutDto
        {
            FlightId = flight.Id,
            FlightNumber = flight.FlightNumber,
            AircraftName = aircraft?.Name ?? "Sovereign Aircraft",
            Capacity = aircraft?.Capacity ?? (firstCount + busCount + econCount),
            FirstClassSeats = firstCount,
            BusinessSeats = busCount,
            EconomySeats = econCount,
            Seats = seatDetails,
            OccupiedSeats = occupiedSeats
        };
    }
    #endregion
}


