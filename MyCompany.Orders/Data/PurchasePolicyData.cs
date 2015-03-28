using System.Collections.Generic;
using NServiceBus.Saga;

namespace MyCompany.Orders.Data
{
    public class PurchasePolicyData : ContainSagaData
    {
        [Unique]
        public string CustomerId { get; set; }

        public List<Order> Orders { get; set; }

        public bool CustomerIsPreferred { get; set; }

        public bool OrderCancelled { get; set; }

        public double Balance { get; set; }
    }
}
