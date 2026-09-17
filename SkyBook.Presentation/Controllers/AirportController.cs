using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Controllers
{
    public class AirportController : Controller
    {
        private readonly IAirportService _airportService;
        public AirportController(IAirportService airportService)
        {
            _airportService = airportService;
        }
        #region Index
        public async Task<IActionResult> Index()
        {
            var airports = await _airportService.GetAllAirportAsync();
            return View(airports);
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var airport = await _airportService.GetAirportByIdAsync(id);
            if (airport == null)
            {
                return NotFound();
            }
            return View(airport);
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
        public async Task<IActionResult> Create(AirportVM airportvm)
        {
            if (ModelState.IsValid)
            {
                await _airportService.CreateAsync(airportvm);
                return RedirectToAction(nameof(Index));
            }
            return View(airportvm);
        }
        #endregion
        
        #region EditPost
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var airport = await _airportService.GetAirportByIdAsync(id);
            if (airport == null)
            {
                return NotFound();
            }
            return View(airport);
        }
        #endregion
        
        #region EditPost
        [HttpPost]
        public async Task<IActionResult> Edit(AirportVM airportvm)
        {
            if (ModelState.IsValid)
            {
                await _airportService.UpdateAirportAsync(airportvm);
                return RedirectToAction(nameof(Index));
            }
            return View(airportvm);
        }
        #endregion
        
        #region Delete
        [HttpPost]
        public async Task<IActionResult>Delete(int id)
        {
            await _airportService.DeleteAirportAsync(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion



    }
}
