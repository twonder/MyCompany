using System;

namespace MyCompany.Orders.Data
{
    public class Order
    {
        public virtual Guid Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual double Amount { get; set; }
    }
}
