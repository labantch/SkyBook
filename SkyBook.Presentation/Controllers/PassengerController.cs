using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;

namespace SkyBook.Presentation.Controllers
{
    public class PassengerController : Controller
    {
        private readonly IPassengerService? _passengerService;

        public PassengerController(
            IPassengerService? passengerService = null)
        {
            _passengerService = passengerService;
        }

        #region Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            if (_passengerService == null) return View(new List<PassengerVM>());
            var passengers = await _passengerService.GetAllPassengersAsync();
            return View(passengers);
        }
        #endregion

        #region Details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                try
                {
                    var passenger = await _passengerService.GetPassengerByIdAsync(id.Value);
                    return View(passenger);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return View();
        }
        #endregion
    }
}
