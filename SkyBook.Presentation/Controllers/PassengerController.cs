using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Controllers
{
    public class PassengerController : Controller
    {
        private readonly IPassengerService _passengerService;

        public PassengerController(IPassengerService passengerService)
        {
            _passengerService = passengerService;
        }

        public async Task<IActionResult> Index()
        {
            var passenger = await _passengerService.GetAllPassengersAsync();
            return View(passenger);
        }

        public async Task<IActionResult> Details(int id)
        {
            var passenger = await _passengerService.GetPassengersByIdAsync(id);
            if (passenger == null)
            {
                return NotFound();
            }

            return View(passenger);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PassengerVM passenger)
        {
            if (ModelState.IsValid)
            {
                await _passengerService.AddPassengerAsync(passenger);
                return RedirectToAction(nameof(Index));
            }

            return View(passenger);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var passenger = await _passengerService.GetPassengersByIdAsync(id);
            if (passenger == null)
            {
                return NotFound();
            }
            return View(passenger);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PassengerVM passengerVm)
        {
            if (ModelState.IsValid)
            {
                await _passengerService.UpdatePassengerAsync(passengerVm);
                return RedirectToAction(nameof(Index));
            }

            return View(passengerVm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _passengerService.DeletePassengerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
