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

           
            public async Task<IActionResult> Index()
            {
                var flights = await _flightService.GetAllFlightsAsync();

                return View(flights);
            }

           
            public async Task<IActionResult> Details(int id)
            {
                var flight = await _flightService.GetFlightByIdAsync(id);

                if (flight == null)
                    return NotFound();

                return View(flight);
            }

           
            [HttpGet]
            public IActionResult Create()
            {
                return View();
            }

           
            [HttpPost]
            public async Task<IActionResult> Create(FlightVM model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _flightService.CreateFlightAsync(model);

                return RedirectToAction(nameof(Index));
            }

            
            [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                var flight = await _flightService.GetFlightForEditAsync(id);

                if (flight == null)
                    return NotFound();

                return View(flight);
            }

          
            [HttpPost]
            public async Task<IActionResult> Edit(FlightVM model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                await _flightService.UpdateFlightAsync(model);

                return RedirectToAction(nameof(Index));
            }

           
            [HttpPost]
            public async Task<IActionResult> Delete(int id)
            {
                await _flightService.DeleteFlightAsync(id);

                return RedirectToAction(nameof(Index));
            }

           
            [HttpPost]
            public async Task<IActionResult> ChangeStatus(int id, FlightStatus status)
            {
                await _flightService.ChangeStatusAsync(id, status);

                return RedirectToAction(nameof(Index));
            }

            [HttpGet]
            public IActionResult Search()
            {
                return View(new FlightSearshVM());
            }

          
            [HttpPost]
            public async Task<IActionResult> Search(FlightSearshVM model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var flights = await _flightService.SearchFlightsAsync(model);

                return View("SearchResults", flights);
            }
        }
    }

        

