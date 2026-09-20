using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.PaymentGateway
{
    public class PaymentResult
    {
        public bool IsSuccess {  get; set; }
        public string? TransactionId {  get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PaymentUrl {  get; set; }
    }
}
