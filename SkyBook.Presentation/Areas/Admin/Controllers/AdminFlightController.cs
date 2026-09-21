using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminFlightController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly IAirportService _airportService;
        private readonly IAircraftService _aircraftService;

        public AdminFlightController(
            IFlightService flightService,
            IAirportService airportService,
            IAircraftService aircraftService)
        {
            _flightService = flightService;
            _airportService = airportService;
            _aircraftService = aircraftService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            ViewBag.CurrentPage = page;
            var flights = await _flightService.GetAllFlightsAsync();

            ViewBag.Airports = await _airportService.GetAllAirportAsync();
            ViewBag.Aircrafts = await _aircraftService.GetAllAircraftsAsync();

            return View(flights);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlightVM flight, int returnPage = 1)
        {
            if (flight.Stops != null && flight.Stops.Any())
            {
                flight.Stops = flight.Stops.Where(s => s.AirportId > 0).ToList();
                for (int i = 0; i < flight.Stops.Count; i++)
                {
                    flight.Stops[i].StopOrder = i + 1;
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid flight details.";
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }

            try
            {
                await _flightService.CreateFlightAsync(flight);
                TempData["Success"] = $"Flight {flight.FlightNumber} created successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FlightVM flight, int returnPage = 1)
        {
            if (flight.Stops != null && flight.Stops.Any())
            {
                flight.Stops = flight.Stops.Where(s => s.AirportId > 0).ToList();
                for (int i = 0; i < flight.Stops.Count; i++)
                {
                    flight.Stops[i].StopOrder = i + 1;
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid flight details.";
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }

            try
            {
                await _flightService.UpdateFlightAsync(flight);
                TempData["Success"] = $"Flight {flight.FlightNumber} updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int returnPage = 1)
        {
            try
            {
                await _flightService.DeleteFlightAsync(id);
                TempData["Success"] = "Flight deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, FlightStatus status, int returnPage = 1)
        {
            try
            {
                await _flightService.ChangeStatusAsync(id, status);
                TempData["Success"] = $"Flight status changed to {status}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }
    }
}

