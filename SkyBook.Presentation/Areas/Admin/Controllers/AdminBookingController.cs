using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminBookingController : Controller
    {
        private readonly IBookingService _bookingService;

        public AdminBookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();

            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var status = await _bookingService.ToggleStatusAsync(id);
                var message = status == BookingStatus.Cancelled
                    ? "Booking canceled successfully."
                    : "Booking restored successfully.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        success = true,
                        status = status.ToString(),
                        statusInt = (int)status,
                        message
                    });
                }

                TempData["Success"] = message;
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }

                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _bookingService.CancelAsync(id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        success = true,
                        status = BookingStatus.Cancelled.ToString(),
                        statusInt = (int)BookingStatus.Cancelled,
                        message = "Booking canceled successfully."
                    });
                }

                TempData["Success"] = "Booking canceled successfully.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }

                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Uncancel(int id)
        {
            try
            {
                await _bookingService.UncancelAsync(id);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        success = true,
                        status = BookingStatus.Confirmed.ToString(),
                        statusInt = (int)BookingStatus.Confirmed,
                        message = "Booking restored successfully."
                    });
                }

                TempData["Success"] = "Booking restored successfully.";
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }

                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

