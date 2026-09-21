using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(
            IBookingService bookingService,
            UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        #region MyBookings
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = string.IsNullOrEmpty(userId) 
                ? new List<MyBookingVM>() 
                : await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id, userId);
            return View(booking);
        }
        #endregion

        #region CreateGet
        [HttpGet]
        public IActionResult Create(int flightId, int seatId)
        {
            var model = new CreateBookingVM
            {
                FlightId = flightId,
                SeatId = seatId
            };
            return View(model);
        }
        #endregion

        #region CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            try
            {
                await _bookingService.CreateBookingAsync(userId, model);

                TempData["Success"] = "Booking created successfully.";

                return RedirectToAction(nameof(MyBookings));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        #endregion

        #region Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            try
            {
                await _bookingService.CancelAsync(id, userId);

                TempData["Success"] = "Booking cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(MyBookings));
        }
        #endregion

        #region SelectSeats
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SelectSeats()
        {
            return View();
        }
        #endregion

        #region ConfirmPayment
        [HttpGet]
        public IActionResult ConfirmPayment()
        {
            return View();
        }
        #endregion

        #region ConfirmBooking 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(ConfirmBookingDto model)
        {
            if (model == null)
            {
                TempData["Error"] = "Invalid booking details provided.";
                return RedirectToAction(nameof(MyBookings));
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(ConfirmPayment)) });
            }

            try
            {
                var result = await _bookingService.ConfirmBookingAsync(userId, model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "Booking failed.";
                    return RedirectToAction(nameof(MyBookings));
                }

                int bookingId = result.BookingId;

                var paymentMethod = PaymentMethod.Card;

                // Redirect to Paymob payment gateway
                return RedirectToAction("Pay", "Payment", new
                {
                    bookingId,
                    paymentMethod
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(MyBookings));
            }
        }
        #endregion

        #region GetOccupiedSeats
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetOccupiedSeats(int? flightId, string? flightNumber)
        {
            var seats = await _bookingService.GetOccupiedSeatNumbersAsync(flightId, flightNumber);
            return Json(seats);
        }
        #endregion

        #region GetFlightSeatLayout
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetFlightSeatLayout(int? flightId, string? flightNumber)
        {
            var layout = await _bookingService.GetFlightSeatLayoutAsync(flightId, flightNumber);
            if (layout == null)
            {
                return NotFound(new { message = "Flight not found or has no seat configuration." });
            }
            return Json(layout);
        }
        #endregion
    }
}
