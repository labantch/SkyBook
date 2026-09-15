using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;


namespace SkyBook.Presentation.Controllers
{
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
        public async Task<IActionResult>Details(int id)
        {
            var aircraft=await _aircraftService.GetAircraftByIdAsync(id);
            if (aircraft == null)
            {
                return NotFound();
            }
            return View(aircraft);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>Create(AircraftVM aircraftvm)
        {
            if (ModelState.IsValid)
            {
                await _aircraftService.CreateAircraftAsync(aircraftvm);
                return RedirectToAction(nameof(Index));
            }
            return View(aircraftvm);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var aircraft = await _aircraftService.GetAircraftByIdAsync(id);
            if (aircraft == null)
            {
                return NotFound();
            }
            return View(aircraft);
        }
        [HttpPost]
        public async Task<IActionResult>Edit(AircraftVM aircraftvm)
        {
            if (ModelState.IsValid) 
            {
                await _aircraftService.UpdateAircraftAsync(aircraftvm);
                return RedirectToAction(nameof(Index));
            };
            return View(aircraftvm);
        }
        public async Task<IActionResult>Delete(int id)
        {
            await _aircraftService.DeleteAircraftAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
