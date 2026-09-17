using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Controllers
{
       
        [Authorize(Roles = "Admin")]
        public class PassengerController : Controller
        {
            private readonly IPassengerService _passengerService;

            public PassengerController(
                IPassengerService passengerService)
            {
                _passengerService = passengerService;
            }
        #region Index
        public async Task<IActionResult> Index()
            {
                var passengers = await _passengerService.GetAllPassengersAsync();
                return View(passengers);
            }
        #endregion


        #region Details
        public async Task<IActionResult> Details(int id)
            {
                try
                {
                    var passenger =await _passengerService.GetPassengerByIdAsync(id);
                    return View(passenger);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
        #endregion
    }
}
