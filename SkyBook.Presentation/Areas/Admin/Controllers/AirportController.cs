using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AirportController : Controller
    {
        private readonly IAirportService _airportService;

        public AirportController(IAirportService airportService)
        {
            _airportService = airportService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            ViewBag.CurrentPage = page;
            var airports = await _airportService.GetAllAirportAsync();

            return View(airports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AirportVM airport, int returnPage = 1)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid airport data.";
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }

            try
            {
                await _airportService.CreateAsync(airport);
                TempData["Success"] = $"Airport '{airport.Name}' successfully added.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirportVM airport, int returnPage = 1)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid airport data.";
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }

            try
            {
                await _airportService.UpdateAirportAsync(airport);
                TempData["Success"] = $"Airport '{airport.Name}' updated successfully.";
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
                await _airportService.DeleteAirportAsync(id);
                TempData["Success"] = "Airport deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }
    }
}

