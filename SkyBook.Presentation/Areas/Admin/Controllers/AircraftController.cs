using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AircraftController : Controller
    {
        private readonly IAircraftService _aircraftService;

        public AircraftController(IAircraftService aircraftService)
        {
            _aircraftService = aircraftService;
        }

        public async Task<IActionResult> Index()
        {
            var aircrafts = await _aircraftService.GetAllAircraftsAsync();

            return View(aircrafts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AircraftVM aircraft)
        {
            SetCapacity(aircraft);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid aircraft data.";
                return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AircraftVM aircraft)
        {
            SetCapacity(aircraft);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter valid aircraft data.";
                return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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

            return RedirectToAction(nameof(Index));
        }

        private void SetCapacity(AircraftVM aircraft)
        {
            aircraft.Capacity = aircraft.EconomySeats
                + aircraft.BusinessSeats
                + aircraft.FirstClassSeats;
        }
    }
}

