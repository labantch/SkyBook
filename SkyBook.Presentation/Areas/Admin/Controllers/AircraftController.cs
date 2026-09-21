using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AircraftController : Controller
    {
        private readonly IAircraftService _aircraftService;

        public AircraftController(IAircraftService aircraftService)
        {
            _aircraftService = aircraftService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1)
            {
                page = 1;
            }

            ViewBag.CurrentPage = page;
            var aircrafts = await _aircraftService.GetAllAircraftsAsync();

            return View(aircrafts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AircraftVM aircraft, int returnPage = 1)
        {
            SetCapacity(aircraft);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid aircraft data.";
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }

            try
            {
                await _aircraftService.CreateAsync(aircraft);
                TempData["Success"] = $"Aircraft '{aircraft.Name}' successfully added.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AircraftVM aircraft, int returnPage = 1)
        {
            SetCapacity(aircraft);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid aircraft data.";
                return RedirectToAction(nameof(Index), new { page = returnPage });
            }

            try
            {
                await _aircraftService.UpdateAsync(aircraft);
                TempData["Success"] = $"Aircraft '{aircraft.Name}' updated successfully.";
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
                await _aircraftService.DeleteAsync(id);
                TempData["Success"] = "Aircraft deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { page = returnPage });
        }

        private void SetCapacity(AircraftVM aircraft)
        {
            aircraft.Capacity = aircraft.EconomySeats
                + aircraft.BusinessSeats
                + aircraft.FirstClassSeats;
        }
    }
}

