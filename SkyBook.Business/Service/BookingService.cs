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
    public async Task<int> CreateBookingAsync(string userId, CreateBookingVM model)
    {
        var flight = await _context.Flights
            .FirstOrDefaultAsync(f => f.Id == model.FlightId);
        if (flight == null)
            throw new Exception("Flight not found.");

        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.Id == model.SeatId && s.AircraftId == flight.AircraftId);
        if (seat == null)
            throw new Exception("Seat not found for this aircraft.");

        var isBooked = await IsSeatAvailableAsync(model.FlightId, model.SeatId);
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

        var bookingReference = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

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

        var groups = bookings.GroupBy(b =>
            ExtractBaseRef(b.BookingReference) is { Length: > 0 } br ? br : b.BookingReference);

        var result = new List<MyBookingVM>();

        foreach (var g in groups)
        {
            var first = g.First();
            var user = first.User;

            var segments = g
                .GroupBy(b => b.FlightId)
                .OrderBy(fg => fg.First().Flight?.DepartureTime ?? DateTime.MinValue)
                .Select(fg =>
                {
                    var fb = fg.First();
                    var fl = fb.Flight;
                    var seats = string.Join(", ",
                        fg.Select(b => b.Seat?.SeatNumber)
                          .Where(s => !string.IsNullOrEmpty(s))
                          .Distinct());
                    return new BookingSegmentVm
                    {
                        FlightId = fl?.Id ?? 0,
                        FlightNumber = fl?.FlightNumber ?? "",
                        DepartureAirport = fl?.DepartureAirport?.Name ?? "",
                        ArrivalAirport = fl?.ArrivalAirport?.Name ?? "",
                        DepartureTime = fl?.DepartureTime ?? DateTime.MinValue,
                        ArrivalTime = fl?.ArrivalTime ?? DateTime.MinValue,
                        Price = fg.Sum(b => b.TotalPrice),
                        SeatNumbers = seats
                    };
                })
                .ToList();

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

            var firstSeg = segments.FirstOrDefault();
            var combinedSeats = string.Join(", ",
                g.Select(b => b.Seat?.SeatNumber).Where(s => !string.IsNullOrEmpty(s)).Distinct());

            result.Add(new MyBookingVM
            {
                BookingId = first.Id,
                BookingReference = g.Key,
                FlightNumber = firstSeg?.FlightNumber ?? "",
                DepartureAirPort = firstSeg?.DepartureAirport ?? "",
                ArrivalAirPort = firstSeg?.ArrivalAirport ?? "",
                DepartureTime = firstSeg?.DepartureTime ?? DateTime.Now,
                ArrivalTime = firstSeg?.ArrivalTime ?? DateTime.Now,
                SeatNumber = !string.IsNullOrEmpty(combinedSeats) ? combinedSeats : "",
                Status = g.Any(b => b.Status == BookingStatus.Confirmed)
                                     ? BookingStatus.Confirmed : first.Status,
                TotalPrice = g.Sum(b => b.TotalPrice),
                PassengerName = passengers.FirstOrDefault()?.FullName ?? "",
                BookingDate = first.BookingDate,
                UserName = user?.UserName ?? "",
                UserFullName = !string.IsNullOrWhiteSpace(user?.FullName)
                                     ? user.FullName : (user?.UserName ?? "Valued Customer"),
                UserImageUrl = user?.ImageUrl,
                Passengers = passengers,
                Segments = segments
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
            .FirstOrDefaultAsync(b => b.Id == bookingId &&
                                      (string.IsNullOrEmpty(userId) || b.UserId == userId));

        if (booking == null)
            throw new Exception("Booking not found.");

        string baseRef = ExtractBaseRef(booking.BookingReference);

        var sisterBookings = await _context.Bookings
            .Include(b => b.Passenger)
            .Include(b => b.Seat)
            .Include(b => b.Flight)
                .ThenInclude(f => f.DepartureAirport)
            .Include(b => b.Flight)
                .ThenInclude(f => f.ArrivalAirport)
            .Where(b => b.UserId == booking.UserId &&
                        (b.BookingReference == booking.BookingReference ||
                         (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef + "-"))))
            .ToListAsync();

        if (sisterBookings.Count == 0)
            sisterBookings = new List<Booking> { booking };

        var segments = sisterBookings
            .GroupBy(b => b.FlightId)
            .OrderBy(fg => fg.First().Flight?.DepartureTime ?? DateTime.MinValue)
            .Select(fg =>
            {
                var fb = fg.First();
                var fl = fb.Flight;
                var seats = string.Join(", ",
                    fg.Select(b => b.Seat?.SeatNumber)
                      .Where(s => !string.IsNullOrEmpty(s))
                      .Distinct());
                return new BookingSegmentVm
                {
                    FlightId = fl?.Id ?? 0,
                    FlightNumber = fl?.FlightNumber ?? "",
                    DepartureAirport = fl?.DepartureAirport?.Name ?? "",
                    ArrivalAirport = fl?.ArrivalAirport?.Name ?? "",
                    DepartureTime = fl?.DepartureTime ?? DateTime.MinValue,
                    ArrivalTime = fl?.ArrivalTime ?? DateTime.MinValue,
                    Price = fg.Sum(b => b.TotalPrice),
                    SeatNumbers = seats
                };
            })
            .ToList();

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

        var combinedSeats = string.Join(", ",
            sisterBookings.Select(b => b.Seat?.SeatNumber)
                          .Where(s => !string.IsNullOrEmpty(s)).Distinct());

        var firstSeg = segments.FirstOrDefault();

        return new BookingDetailsVm
        {
            BookingId = booking.Id,
            BookingDate = booking.BookingDate,
            TotalPrice = sisterBookings.Sum(b => b.TotalPrice),
            Status = booking.Status,
            BookingReference = !string.IsNullOrEmpty(baseRef) ? baseRef : booking.BookingReference,
            DepartureAirPort = firstSeg?.DepartureAirport ?? "",
            ArrivalAirPort = firstSeg?.ArrivalAirport ?? "",
            PassengerName = passengers.FirstOrDefault()?.FullName
                               ?? $"{booking.Passenger?.FirstName} {booking.Passenger?.LastName}",
            FlightNumber = firstSeg?.FlightNumber ?? "",
            SeatNumber = !string.IsNullOrEmpty(combinedSeats) ? combinedSeats : "",
            UserImageUrl = booking.User?.ImageUrl,
            UserName = booking.User?.UserName ?? "",
            UserFullName = !string.IsNullOrWhiteSpace(booking.User?.FullName)
                                 ? booking.User.FullName : (booking.User?.UserName ?? "Valued Customer"),
            UserEmail = booking.User?.Email ?? "",
            Passengers = passengers,
            Segments = segments
        };
    }
    #endregion

    #region Cancel
    public async Task CancelAsync(int bookingId, string userId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
        if (booking == null)
            throw new Exception("Booking not found.");
        if (booking.Status == BookingStatus.Cancelled)
            throw new Exception("Booking is already cancelled.");

        string baseRef = ExtractBaseRef(booking.BookingReference);

        var related = await _context.Bookings
            .Where(b => b.UserId == userId &&
                        (b.BookingReference == booking.BookingReference ||
                         (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef + "-"))))
            .ToListAsync();

        foreach (var b in related)
            b.Status = BookingStatus.Cancelled;

        await _context.SaveChangesAsync();
    }
    #endregion

    #region IsSeatAvailable
    public async Task<bool> IsSeatAvailableAsync(int flightId, int seatId)
    {
        var isBooked = await _context.Bookings.AnyAsync(b =>
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

        var totalSeats = await _context.Seats.CountAsync(s => s.AircraftId == flight.AircraftId);
        var bookedSeats = await _context.Bookings.CountAsync(b =>
            b.FlightId == flightId && b.Status != BookingStatus.Cancelled);
        return totalSeats - bookedSeats;
    }
    #endregion

    #region GetAllBookings
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
            BaseRef = ExtractBaseRef(b.BookingReference) is { Length: > 0 } br ? br : b.BookingReference,
            b.UserId
        });

        var result = new List<BookingDetailsVm>();

        foreach (var g in groups)
        {
            var first = g.First();
            var user = first.User;
            var userFullName = !string.IsNullOrWhiteSpace(user?.FullName)
                                 ? user.FullName : (user?.UserName ?? "Valued Customer");

            var segments = g
                .GroupBy(b => b.FlightId)
                .OrderBy(fg => fg.First().Flight?.DepartureTime ?? DateTime.MinValue)
                .Select(fg =>
                {
                    var fb = fg.First();
                    var fl = fb.Flight;
                    var seats = string.Join(", ",
                        fg.Select(b => b.Seat?.SeatNumber)
                          .Where(s => !string.IsNullOrEmpty(s))
                          .Distinct());
                    return new BookingSegmentVm
                    {
                        FlightId = fl?.Id ?? 0,
                        FlightNumber = fl?.FlightNumber ?? "",
                        DepartureAirport = fl?.DepartureAirport?.Name ?? "",
                        ArrivalAirport = fl?.ArrivalAirport?.Name ?? "",
                        DepartureTime = fl?.DepartureTime ?? DateTime.MinValue,
                        ArrivalTime = fl?.ArrivalTime ?? DateTime.MinValue,
                        Price = fg.Sum(b => b.TotalPrice),
                        SeatNumbers = seats
                    };
                })
                .ToList();

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

            var combinedSeats = string.Join(", ",
                g.Select(b => b.Seat?.SeatNumber).Where(s => !string.IsNullOrEmpty(s)).Distinct());
            var status = g.Any(b => b.Status == BookingStatus.Confirmed)
                           ? BookingStatus.Confirmed : first.Status;
            var firstSeg = segments.FirstOrDefault();

            result.Add(new BookingDetailsVm
            {
                BookingId = first.Id,
                BookingDate = first.BookingDate,
                TotalPrice = g.Sum(b => b.TotalPrice),
                Status = status,
                BookingReference = g.Key.BaseRef,
                DepartureAirPort = firstSeg?.DepartureAirport ?? "",
                ArrivalAirPort = firstSeg?.ArrivalAirport ?? "",
                PassengerName = passengers.FirstOrDefault()?.FullName ?? "Unknown Passenger",
                FlightNumber = firstSeg?.FlightNumber ?? "",
                SeatNumber = !string.IsNullOrEmpty(combinedSeats) ? combinedSeats : "",
                UserImageUrl = user?.ImageUrl,
                UserName = user?.UserName ?? "",
                UserFullName = userFullName,
                UserEmail = user?.Email ?? "",
                Passengers = passengers,
                Segments = segments
            });
        }

        return result;
    }
    #endregion

    #region Convenience overloads
    public async Task<BookingDetailsVm> GetBookingByIdAsync(int bookingId)
        => await GetBookingByIdAsync(bookingId, "");

    public async Task CancelAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");
        await CancelAsync(bookingId, booking.UserId);
    }
    #endregion

    #region ToggleStatus
    public async Task<BookingStatus> ToggleStatusAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");

        var newStatus = (booking.Status == BookingStatus.Cancelled)
            ? BookingStatus.Confirmed
            : BookingStatus.Cancelled;

        string baseRef = ExtractBaseRef(booking.BookingReference);

        var related = await _context.Bookings
            .Where(b => b.UserId == booking.UserId &&
                        (b.BookingReference == booking.BookingReference ||
                         (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef + "-"))))
            .ToListAsync();

        foreach (var b in related)
            b.Status = newStatus;

        await _context.SaveChangesAsync();
        return newStatus;
    }
    #endregion

    #region UncancelAsync
    public async Task UncancelAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
            throw new Exception("Booking not found.");

        string baseRef = ExtractBaseRef(booking.BookingReference);

        var related = await _context.Bookings
            .Where(b => b.UserId == booking.UserId &&
                        (b.BookingReference == booking.BookingReference ||
                         (!string.IsNullOrEmpty(baseRef) && b.BookingReference.StartsWith(baseRef + "-"))))
            .ToListAsync();

        foreach (var b in related)
            b.Status = BookingStatus.Confirmed;

        await _context.SaveChangesAsync();
    }
    #endregion

    #region ConfirmBookingAsync
    public async Task<ConfirmBookingResultDto> ConfirmBookingAsync(string? userId, ConfirmBookingDto model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model), "Invalid booking payload.");

        if (string.IsNullOrEmpty(userId))
        {
            var guestEmail = model.Passengers.FirstOrDefault()?.Email;
            if (!string.IsNullOrEmpty(guestEmail))
            {
                var userByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == guestEmail);
                if (userByEmail != null) userId = userByEmail.Id;
            }

            if (string.IsNullOrEmpty(userId))
            {
                var defaultUser = await _context.Users.FirstOrDefaultAsync();
                userId = defaultUser?.Id ?? "28FE6275-FA7D-4239-A890-CDDBA0E43395";
            }
        }

        if (model.Segments == null || model.Segments.Count == 0)
            return new ConfirmBookingResultDto { Success = false, Message = "At least one flight segment is required." };

        if (model.Passengers == null || model.Passengers.Count == 0)
            return new ConfirmBookingResultDto { Success = false, Message = "At least one passenger is required." };

        var resolvedFlights = new List<Flight>();
        for (int sIdx = 0; sIdx < model.Segments.Count; sIdx++)
        {
            var seg = model.Segments[sIdx];
            Flight? flight = null;

            if (seg.FlightId.HasValue && seg.FlightId.Value > 0)
                flight = await _context.Flights
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f => f.Id == seg.FlightId.Value);

            if (flight == null && !string.IsNullOrWhiteSpace(seg.FlightNumber))
            {
                var clean = System.Text.RegularExpressions.Regex
                    .Replace(seg.FlightNumber, @"[^\w\-]", "").Trim();
                flight = await _context.Flights
                    .Include(f => f.Aircraft)
                    .FirstOrDefaultAsync(f =>
                        f.FlightNumber == clean ||
                        f.FlightNumber.Contains(clean) ||
                        clean.Contains(f.FlightNumber));
            }

            if (flight == null)
            {
                var identifier = seg.FlightId.HasValue
                    ? $"FlightId={seg.FlightId}"
                    : (!string.IsNullOrWhiteSpace(seg.FlightNumber)
                        ? $"FlightNumber={seg.FlightNumber}"
                        : $"segment index {sIdx}");
                return new ConfirmBookingResultDto
                {
                    Success = false,
                    Message = $"Flight for segment {sIdx + 1} ({identifier}) could not be found. Please restart your booking."
                };
            }

            resolvedFlights.Add(flight);
        }

        var pnr = !string.IsNullOrWhiteSpace(model.BookingReference)
            ? model.BookingReference.Trim()
            : "SKY-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        if (await _context.Bookings.AnyAsync(b => b.BookingReference == pnr))
            pnr = $"{pnr}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}";

        var claimedSeatIdsByFlight = new Dictionary<int, HashSet<int>>();
        foreach (var flight in resolvedFlights)
        {
            if (!claimedSeatIdsByFlight.ContainsKey(flight.Id))
            {
                var takenIds = await _context.Bookings
                    .Where(b => b.FlightId == flight.Id && b.Status != BookingStatus.Cancelled)
                    .Select(b => b.SeatId)
                    .ToListAsync();
                claimedSeatIdsByFlight[flight.Id] = new HashSet<int>(takenIds);
            }
        }

        var createdBookings = new List<Booking>();

        for (int pIdx = 0; pIdx < model.Passengers.Count; pIdx++)
        {
            var pDto = model.Passengers[pIdx];
            var passportNum = !string.IsNullOrWhiteSpace(pDto.Passport)
                ? pDto.Passport.Trim()
                : "PASS-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var passenger = await _context.Passengers
                .FirstOrDefaultAsync(p => p.PassportNumber == passportNum);

            if (passenger == null)
            {
                DateTime dob = DateTime.TryParse(pDto.Dob, out var parsed) ? parsed : new DateTime(2000, 1, 1);
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
                if (DateTime.TryParse(pDto.Dob, out var updDob)) passenger.DateOfBirth = updDob;
                await _context.SaveChangesAsync();
            }

            for (int sIdx = 0; sIdx < model.Segments.Count; sIdx++)
            {
                var seg = model.Segments[sIdx];
                var flight = resolvedFlights[sIdx];
                var claimedSet = claimedSeatIdsByFlight[flight.Id];

                decimal segmentPrice = seg.Price > 0 ? seg.Price : DeriveFlightPrice(flight);

                string? seatString = (pDto.Seats != null && sIdx < pDto.Seats.Count)
                    ? pDto.Seats[sIdx]
                    : null;

                var seatMatch = !string.IsNullOrEmpty(seatString)
                    ? System.Text.RegularExpressions.Regex.Match(seatString, @"\d+[A-Z]").Value
                    : "";

                SeatClass desiredClass = DeriveDesiredClass(flight, seg.Price);

                Seat? seat = null;
                if (!string.IsNullOrEmpty(seatMatch))
                    seat = await _context.Seats
                        .FirstOrDefaultAsync(s => s.AircraftId == flight.AircraftId &&
                                                  s.SeatNumber == seatMatch &&
                                                  s.Class == desiredClass);

                if (seat == null || claimedSet.Contains(seat.Id))
                {
                    seat = await _context.Seats
                        .Where(s => s.AircraftId == flight.AircraftId
                                 && s.Class == desiredClass
                                 && !claimedSet.Contains(s.Id))
                        .OrderBy(s => s.Id)
                        .FirstOrDefaultAsync();
                }

                if (seat == null)
                {
                    _context.ChangeTracker.Clear();
                    return new ConfirmBookingResultDto
                    {
                        Success = false,
                        Message = $"No available {desiredClass} seats remain on flight {flight.FlightNumber} for passenger {pIdx + 1}."
                    };
                }

                claimedSet.Add(seat.Id);

                var ticketRef = createdBookings.Count == 0
                    ? pnr
                    : $"{pnr}-{createdBookings.Count + 1}";

                var booking = new Booking
                {
                    UserId = userId,
                    PassengerId = passenger.Id,
                    FlightId = flight.Id,
                    SeatId = seat.Id,
                    BookingDate = DateTime.Now,
                    TotalPrice = segmentPrice,
                    BookingReference = ticketRef,
                    Status = BookingStatus.PendingPayment
                };

                _context.Bookings.Add(booking);
                createdBookings.Add(booking);
            }
        }

        if (model.TotalPrice > 0 && createdBookings.Count > 0)
        {
            var currentSum = createdBookings.Sum(b => b.TotalPrice);
            if (currentSum != model.TotalPrice)
            {
                decimal perBooking = Math.Round(model.TotalPrice / createdBookings.Count, 2);
                decimal remainder = model.TotalPrice - (perBooking * createdBookings.Count);
                for (int i = 0; i < createdBookings.Count; i++)
                {
                    createdBookings[i].TotalPrice = perBooking + (i == 0 ? remainder : 0);
                }
            }
        }

        await _context.SaveChangesAsync();

        return new ConfirmBookingResultDto
        {
            Success = true,
            BookingId = createdBookings.FirstOrDefault()?.Id ?? 0,
            BookingReference = pnr,
            BookingsCreated = createdBookings.Count,
            Message = "Reservation successfully persisted."
        };
    }
    #endregion

    #region Helpers
    private static decimal DeriveFlightPrice(Flight flight)
    {
        if (flight.EconomyPrice > 0) return flight.EconomyPrice;
        if (flight.Price > 0) return flight.Price;
        return 0;
    }

    private static SeatClass DeriveDesiredClass(Flight flight, decimal segmentPrice)
    {
        if (flight.FirstClassPrice > 0 && segmentPrice >= flight.FirstClassPrice)
            return SeatClass.FirstClass;
        if (flight.BusinessPrice > 0 && segmentPrice >= flight.BusinessPrice)
            return SeatClass.Business;
        return SeatClass.Economy;
    }

    private static string ExtractBaseRef(string? refStr)
    {
        if (string.IsNullOrWhiteSpace(refStr)) return "";
        return System.Text.RegularExpressions.Regex.Replace(refStr.Trim(), @"-\d+$", "");
    }
    #endregion

    #region GetOccupiedSeatNumbers
    public async Task<List<string>> GetOccupiedSeatNumbersAsync(int? flightId, string? flightNumber)
    {
        var query = _context.Bookings.Where(b => b.Status != BookingStatus.Cancelled);

        if (flightId.HasValue && flightId.Value > 0)
        {
            query = query.Where(b => b.FlightId == flightId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(flightNumber))
        {
            var clean = System.Text.RegularExpressions.Regex
                .Replace(flightNumber, @"[^\w\-]", "").Trim();
            query = query.Where(b =>
                b.Flight.FlightNumber == clean ||
                b.Flight.FlightNumber.Contains(clean) ||
                clean.Contains(b.Flight.FlightNumber));
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

    #region GetFlightSeatLayout
    public async Task<FlightSeatLayoutDto?> GetFlightSeatLayoutAsync(int? flightId, string? flightNumber)
    {
        Flight? flight = null;

        if (flightId.HasValue && flightId.Value > 0)
            flight = await _context.Flights
                .Include(f => f.Aircraft).ThenInclude(a => a.Seats)
                .FirstOrDefaultAsync(f => f.Id == flightId.Value);

        if (flight == null && !string.IsNullOrWhiteSpace(flightNumber))
        {
            var clean = System.Text.RegularExpressions.Regex
                .Replace(flightNumber, @"[^\w\-]", "").Trim();
            flight = await _context.Flights
                .Include(f => f.Aircraft).ThenInclude(a => a.Seats)
                .FirstOrDefaultAsync(f =>
                    f.FlightNumber == clean ||
                    f.FlightNumber.Contains(clean) ||
                    clean.Contains(f.FlightNumber));
        }

        if (flight == null) return null;

        var occupiedSeats = await _context.Bookings
            .Where(b => b.FlightId == flight.Id && b.Status != BookingStatus.Cancelled && b.Seat != null)
            .Select(b => b.Seat.SeatNumber)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToListAsync();

        var aircraft = flight.Aircraft;
        var seats = aircraft?.Seats?.OrderBy(s => s.Id).ToList() ?? new List<Seat>();

        var seatDetails = seats.Select(s =>
        {
            var m = System.Text.RegularExpressions.Regex.Match(s.SeatNumber ?? "", @"^(\d+)([A-Z])$");
            int.TryParse(m.Groups[1].Value, out int row);
            string cabin = s.Class switch
            {
                SeatClass.FirstClass => "first",
                SeatClass.Business => "business",
                _ => "economy"
            };
            return new SeatDetailDto
            {
                SeatNumber = s.SeatNumber ?? "",
                CabinClass = cabin,
                Row = row,
                Letter = m.Groups[2].Value
            };
        }).ToList();

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
