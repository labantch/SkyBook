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
        #region Index
        public async Task<IActionResult> Index() 
        {
            var aircrafts = await _aircraftService.GetAllAircraftsAsync();
            return View(aircrafts);
        }
        #endregion

        #region Details
        public async Task<IActionResult>Details(int id)
        {
            var aircraft=await _aircraftService.GetAircraftByIdAsync(id);
            if (aircraft == null)
            {
                return NotFound();
            }
            return View(aircraft);
        }
        #endregion

        #region CreateGet
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        #endregion

        #region CreatPost
        [HttpPost]
        public async Task<IActionResult>Create(AircraftVM aircraftvm)
        {
            if (ModelState.IsValid)
            {
                await _aircraftService.CreateAsync(aircraftvm);
                return RedirectToAction(nameof(Index));
            }
            return View(aircraftvm);
        }
        #endregion
        
        #region EditGet
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
        #endregion
        
        #region EditPost
        [HttpPost]
        public async Task<IActionResult>Edit(AircraftVM aircraftvm)
        {
            if (ModelState.IsValid) 
            {
                await _aircraftService.UpdateAsync(aircraftvm);
                return RedirectToAction(nameof(Index));
            };
            return View(aircraftvm);
        }
        #endregion
        
        #region Delete
        [HttpPost]
        public async Task<IActionResult>Delete(int id)
        {
            await _aircraftService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
