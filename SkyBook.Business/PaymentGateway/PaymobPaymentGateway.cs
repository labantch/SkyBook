using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using SkyBook.Business.PaymentGateway;

namespace SkyBook.Business.PaymentGateway
{
    public class PaymobPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaymobPaymentGateway(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            PaymentRequest request)
        {
            var secretKey =
                _configuration["Paymob:ApiKey"];

            var integrationId =
                _configuration["Paymob:IntegrationId"];
            var hmacSecret = _configuration["Paymob:Hmac"];

            var baseUrl =
                _configuration["Paymob:BaseUrl"];

            var amountInCents =
                (int)(request.Amount * 100);

            var body = new
            {
                amount = amountInCents,
                currency = "EGP",

                payment_methods = new[]
                {
                    int.Parse(integrationId!)
                },
                notification_url=_configuration["Paymob:NotificationUrl"],

                items = new[]
                {
                    new
                    {
                        name = "SkyBook Flight Booking",
                        amount = amountInCents,
                        description = "Flight booking payment",
                        quantity = 1
                    }
                },

                billing_data = new
                {
                    first_name = request.CustomerName,
                    last_name = "Customer",
                    phone_number = request.CustomerPhone,
                    email = request.CustomerEmail,

                    apartment = "NA",
                    street = "NA",
                    building = "NA",
                    city = "NA",
                    country = "EG",
                    floor = "NA",
                    state = "NA"
                }
            };

            var json = JsonSerializer.Serialize(body);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Token",
                    secretKey);

            var response = await _httpClient.PostAsync(
                $"{baseUrl}/v1/intention/",
                content);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = responseContent
                };
            }

            var result =
                JsonSerializer.Deserialize<JsonElement>(
                    responseContent);

            var clientSecret =
                result.GetProperty("client_secret").GetString();

            return new PaymentResult
            {
                IsSuccess = true,
                PaymentUrl =
                    $"{baseUrl}/unifiedcheckout/?publicKey={_configuration["Paymob:PublicKey"]}&clientSecret={clientSecret}",
                Message = "Payment intention created successfully."
            };
        }
    }
}