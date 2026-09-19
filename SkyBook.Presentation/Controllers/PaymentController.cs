using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;
using System.Text.Json;

namespace SkyBook.Presentation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(
            int bookingId,
            PaymentMethod paymentMethod)
        {
            var result = await _paymentService.CreatePaymentAsync(
                bookingId,
                paymentMethod);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("MyBookings", "Booking");
            }

            return Redirect(result.PaymentUrl!);
        }
        [HttpPost]
        [Route("payment/webhook")]
        public async Task<IActionResult> Webhook(
    [FromBody] JsonElement data)
        {
            var transaction = data.GetProperty("obj");

            var transactionId =
                transaction.GetProperty("id").GetInt32().ToString();

            var success =
                transaction.GetProperty("success").GetBoolean();

            var status = success
                ? PaymentStatus.Paid
                : PaymentStatus.Failed;

            await _paymentService.UpdatePaymentStatusAsync(
                transactionId,
                status);

            return Ok();
        }
    }
}