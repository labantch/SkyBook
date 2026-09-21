using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SkyBook.Presentation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;


        public PaymentController(IPaymentService paymentService, IConfiguration configuration)

        {
            _paymentService = paymentService;
            _configuration = configuration;
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
        private bool VerifyHmac(
    JsonElement transaction,
    string receivedHmac)
        {
            var hmacSecret =
                _configuration["Paymob:Hmac"] ?? _configuration["Paymob:HmacSecret"];

            if (string.IsNullOrEmpty(hmacSecret))
                return false;

            var values = new[]
            {
        transaction.GetProperty("amount_cents").ToString(),
        transaction.GetProperty("created_at").ToString(),
        transaction.GetProperty("currency").ToString(),
        transaction.GetProperty("error_occured").ToString(),
        transaction.GetProperty("has_parent_transaction").ToString(),
        transaction.GetProperty("id").ToString(),
        transaction.GetProperty("integration_id").ToString(),
        transaction.GetProperty("is_3d_secure").ToString(),
        transaction.GetProperty("is_auth").ToString(),
        transaction.GetProperty("is_capture").ToString(),
        transaction.GetProperty("is_refunded").ToString(),
        transaction.GetProperty("is_standalone_payment").ToString(),
        transaction.GetProperty("is_void").ToString(),
        transaction.GetProperty("is_voided").ToString(),
        transaction.GetProperty("order").GetProperty("id").ToString(),
        transaction.GetProperty("owner").ToString(),
        transaction.GetProperty("pending").ToString(),
        transaction.GetProperty("source_data").GetProperty("pan").ToString(),
        transaction.GetProperty("source_data").GetProperty("sub_type").ToString(),
        transaction.GetProperty("source_data").GetProperty("type").ToString(),
        transaction.GetProperty("success").ToString()
    };

            var concatenatedValues = string.Concat(values);

            using var hmac =
                new HMACSHA512(
                    Encoding.UTF8.GetBytes(hmacSecret));

            var hash =
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(concatenatedValues));

            var calculatedHmac =
                Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(calculatedHmac),
                Encoding.UTF8.GetBytes(receivedHmac.ToLowerInvariant()));
        }


        [HttpPost]
        [Route("payment/webhook")]
        public async Task<IActionResult> Webhook(
       [FromBody] JsonElement data)
        {
            var transaction = data.GetProperty("obj");

            var receivedHmac =
                Request.Query["hmac"].ToString();

            if (!VerifyHmac(transaction, receivedHmac))
            {
                return Unauthorized();
            }

            var transactionId =
                transaction.GetProperty("id")
                           .GetInt32()
                           .ToString();

            var success =
                transaction.GetProperty("success")
                           .GetBoolean();

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