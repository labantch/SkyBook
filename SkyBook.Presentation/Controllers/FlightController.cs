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
            public async Task<IActionResult> Search(FlightSearshVM model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var flights = await _flightService.SearchFlightsAsync(model);

                return View("SearchResults", flights);
            }
        #endregion
    }
}

        

