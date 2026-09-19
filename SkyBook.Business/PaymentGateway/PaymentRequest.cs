using SkyBook.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.PaymentGateway
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string CustomerName {  get; set; }
        public string CustomerEmail {  get; set; }
        public string CustomerPhone {  get; set; }


    }
}
