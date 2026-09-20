using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Controllers
{
    public class FlightController : Controller
    {
       
            private readonly IFlightService _flightService;

            public FlightController(IFlightService flightService)
            {
                _flightService = flightService;
            }
        #region Index
        public async Task<IActionResult> Index()
            {
                var flights = await _flightService.GetAllFlightsAsync();

                return View(flights);
            }
        #endregion

        #region Details 
        public async Task<IActionResult> Details(int id)
            {
                var flight = await _flightService.GetFlightByIdAsync(id);

                if (flight == null)
                    return NotFound();

                return View(flight);
            }
        #endregion
        
        #region CreateGet
        [HttpGet]
            public IActionResult Create()
            {
                return View();
            }
        #endregion

        #region CreatePost
        [HttpPost]
            public async Task<IActionResult> Create(FlightVM model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _flightService.CreateFlightAsync(model);

                return RedirectToAction(nameof(Index));
            }
        #endregion

        #region EditGet
        [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                var flight = await _flightService.GetFlightForEditAsync(id);

                if (flight == null)
                    return NotFound();

                return View(flight);
            }
        #endregion

        #region EditPost
        [HttpPost]
            public async Task<IActionResult> Edit(FlightVM model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _flightService.UpdateFlightAsync(model);

                return RedirectToAction(nameof(Index));
            }
        #endregion

        #region Delete
        [HttpPost]
            public async Task<IActionResult> Delete(int id)
            {
                await _flightService.DeleteFlightAsync(id);

                return RedirectToAction(nameof(Index));
            }
        #endregion

        #region ChangeStatus
        [HttpPost]
            public async Task<IActionResult> ChangeStatus(int id, FlightStatus status)
            {
                await _flightService.ChangeStatusAsync(id, status);

                return RedirectToAction(nameof(Index));
            }
        #endregion

        #region SearchGet

        [HttpGet]
            public IActionResult Search()
            {
                return View(new FlightSearshVM());
            }
        #endregion

        #region SearchPost
        [HttpPost]
        public IActionResult Search(FlightSearshVM model)
        {
            return RedirectToAction(nameof(Results));
        }
        #endregion

        #region Results
        [HttpGet]
        public async Task<IActionResult> Results(
            string? mode,
            string? origin,
            string? originCity,
            string? destination,
            string? destinationCity,
            string? departure,
            string? departureSub,
            string? depDate,
            string? @return,
            string? returnSub,
            string? retDate,
            string? passengers,
            int? passengerCount,
            string? cabin)
        {
            var allFlights = await _flightService.GetAllFlightsAsync();

            int paxCount = 1;
            if (passengerCount.HasValue && passengerCount.Value > 0)
            {
                paxCount = passengerCount.Value;
            }
            else if (!string.IsNullOrWhiteSpace(passengers))
            {
                var match = Regex.Match(passengers, @"\d+");
                if (match.Success && int.TryParse(match.Value, out var parsedCount) && parsedCount > 0)
                {
                    paxCount = parsedCount;
                }
            }

            DateTime? parsedDepDate = null;
            if (!string.IsNullOrWhiteSpace(depDate) && DateTime.TryParse(depDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dDate))
            {
                parsedDepDate = dDate.Date;
            }
            else if (!string.IsNullOrWhiteSpace(departure))
            {
                var yearMatch = !string.IsNullOrWhiteSpace(departureSub)
                    ? Regex.Match(departureSub, @"\b20\d{2}\b")
                    : null;
                var year = yearMatch != null && yearMatch.Success ? yearMatch.Value : "2026";
                if (DateTime.TryParse($"{departure} {year}", CultureInfo.InvariantCulture, DateTimeStyles.None, out var pDate))
                {
                    parsedDepDate = pDate.Date;
                }
            }

            DateTime? parsedRetDate = null;
            if (!string.IsNullOrWhiteSpace(retDate) && DateTime.TryParse(retDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var rDate))
            {
                parsedRetDate = rDate.Date;
            }
            else if (!string.IsNullOrWhiteSpace(@return))
            {
                var yearMatch = !string.IsNullOrWhiteSpace(returnSub)
                    ? Regex.Match(returnSub, @"\b20\d{2}\b")
                    : null;
                var year = yearMatch != null && yearMatch.Success ? yearMatch.Value : "2026";
                if (DateTime.TryParse($"{@return} {year}", CultureInfo.InvariantCulture, DateTimeStyles.None, out var prDate))
                {
                    parsedRetDate = prDate.Date;
                }
            }

            if (!string.IsNullOrWhiteSpace(origin) && !string.IsNullOrWhiteSpace(destination))
            {
                var originUpper = origin.Trim().ToUpper();
                var destUpper = destination.Trim().ToUpper();
                var isOneWay = string.Equals(mode, "one-way", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(mode, "oneway", StringComparison.OrdinalIgnoreCase);

                var corridorOutbound = allFlights
                    .Where(f =>
                        (string.Equals(f.DepartureAirportCode, originUpper, StringComparison.OrdinalIgnoreCase) ||
                         (!string.IsNullOrWhiteSpace(originCity) && string.Equals(f.DepartureAirportCity, originCity, StringComparison.OrdinalIgnoreCase))) &&
                        (string.Equals(f.ArrivalAirportCode, destUpper, StringComparison.OrdinalIgnoreCase) ||
                         (!string.IsNullOrWhiteSpace(destinationCity) && string.Equals(f.ArrivalAirportCity, destinationCity, StringComparison.OrdinalIgnoreCase))) &&
                        f.AvailableSeats >= paxCount)
                    .ToList();

                var filtered = parsedDepDate.HasValue
                    ? corridorOutbound.Where(f => f.DepartureTime.Date == parsedDepDate.Value).ToList()
                    : corridorOutbound;

                List<FlightVM> returnFlights = new();
                if (!isOneWay)
                {
                    var corridorReturn = allFlights
                        .Where(f =>
                            (string.Equals(f.DepartureAirportCode, destUpper, StringComparison.OrdinalIgnoreCase) ||
                             (!string.IsNullOrWhiteSpace(destinationCity) && string.Equals(f.DepartureAirportCity, destinationCity, StringComparison.OrdinalIgnoreCase))) &&
                            (string.Equals(f.ArrivalAirportCode, originUpper, StringComparison.OrdinalIgnoreCase) ||
                             (!string.IsNullOrWhiteSpace(originCity) && string.Equals(f.ArrivalAirportCity, originCity, StringComparison.OrdinalIgnoreCase))) &&
                            f.AvailableSeats >= paxCount)
                        .ToList();

                    returnFlights = parsedRetDate.HasValue
                        ? corridorReturn.Where(f => f.DepartureTime.Date == parsedRetDate.Value).ToList()
                        : corridorReturn;
                }

                var combined = filtered.Concat(returnFlights).DistinctBy(f => f.Id).ToList();
                return View(combined);
            }

            var generalFiltered = allFlights
                .Where(f =>
                    (!parsedDepDate.HasValue || f.DepartureTime.Date == parsedDepDate.Value) &&
                    f.AvailableSeats >= paxCount)
                .ToList();

            return View(generalFiltered);
        }
        #endregion
    }
}

        

