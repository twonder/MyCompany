using System.Collections.Generic;
using NServiceBus.Saga;

namespace MyCompany.Orders.Data
{
    public class PurchasePolicyData : ContainSagaData
    {
        private List<Order> _orders;
        
        [Unique]
        public string CustomerId { get; set; }

        public List<Order> Orders
        {
            get { return _orders ?? new List<Order>(); }
            set { _orders = value; }
        }

        public bool CustomerIsPreferred { get; set; }

        public double Balance { get; set; }
    }
}
