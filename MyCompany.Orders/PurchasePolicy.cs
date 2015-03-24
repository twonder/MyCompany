using System;
using System.Linq;
using MyCompany.Messages.Events;
using MyCompany.Orders.Data;
using NServiceBus.Saga;
using Order = MyCompany.Orders.Data.Order;

namespace MyCompany.Orders
{
    public class PurchasePolicy : Saga<PurchasePolicyData>,
        IAmStartedByMessages<OrderAccepted>,
        IAmStartedByMessages<MoneyAddedToAccount>
    {
        private double DiscountTotalThreshold = 100;

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PurchasePolicyData> mapper)
        {
            // they key to the policy is customerId
            mapper.ConfigureMapping<OrderAccepted>(order => order.CustomerId)
                .ToSaga(purchasePolicy => purchasePolicy.CustomerId);
        }

        #region Message Handlers

        public void Handle(MoneyAddedToAccount message)
        {
            Data.CustomerId = message.CustomerId;
            Data.Balance += message.Amount;
        }

        public void Handle(OrderAccepted order)
        {
            Data.CustomerId = order.CustomerId;
            // apply the discount
            var amountAfterDiscount = order.Amount - (order.Amount * CalculateDiscount());

            // reject order if not enough money
            if (amountAfterDiscount > Data.Balance)
            {
                RejectOrder(order);
            }

            // update balance
            Data.Balance -= amountAfterDiscount;

            // store the order
            Data.Orders.Add(
                new Order
                {
                    Date = DateTime.Now,
                    Amount = amountAfterDiscount
                });

            // complete the order
            Bus.Publish<OrderCompleted>(o =>
                {
                    o.CustomerId = order.CustomerId;
                    o.OrderId = order.OrderId;
                    o.ProductId = order.ProductId;
                    o.Amount = amountAfterDiscount;
                    o.DateOccurred = DateTime.Now;
                }               
            );
        }
        #endregion

        #region Private Methods
        private double CalculateDiscount()
        {
            if (Data.CustomerIsPreferred)
            {
                // total of orders for preferred timespan
                var total = Data.Orders.Where(o => o.Date > DateTime.Now.AddDays(-14)).Sum(o => o.Amount);

                return total > DiscountTotalThreshold ? 0.20 : 0.10;
            }
            else
            {
                // total of orders for non-preferred timespan
                var total = Data.Orders.Where(o => o.Date > DateTime.Now.AddDays(-7)).Sum(o => o.Amount);

                return total > DiscountTotalThreshold ? 0.10 : 0;
            }
        }

        private void RejectOrder(OrderAccepted order)
        {
            Bus.Publish<OrderRejected>(o =>
            {
                o.CustomerId = order.CustomerId;
                o.OrderId = order.OrderId;
                o.ProductId = order.ProductId;
                o.DateOccurred = DateTime.Now;
            });
        }
        #endregion
    }
}
