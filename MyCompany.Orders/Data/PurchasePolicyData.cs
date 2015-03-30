using System;
using System.Collections.Generic;
using NServiceBus.Saga;

namespace MyCompany.Orders.Data
{
    public class PurchasePolicyData : ContainSagaData
    {
        public virtual Guid Id { get; set; }

        [Unique]
        public virtual string CustomerId { get; set; }

        public virtual List<Order> Orders { get; set; }

        public virtual bool CustomerIsPreferred { get; set; }

        public virtual bool OrderCancelled { get; set; }

        public virtual double Balance { get; set; }
    }
}
