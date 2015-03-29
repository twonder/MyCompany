using System;
using MyCompany.Messages.Events;

namespace MyCompany.Orders.Timeouts
{
    public class BuyersRemorseTimeout
    {
        public DateTime DateOccurred { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public double DiscountApplied { get; set; }
        public double Amount { get; set; }
    }
}
