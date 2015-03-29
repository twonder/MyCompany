using System;
using System.Collections.Generic;
using System.Linq;
using MyCompany.Messages.Commands;
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
            mapper.ConfigureMapping<MoneyAddedToAccount>(order => order.CustomerId)
                .ToSaga(purchasePolicy => purchasePolicy.CustomerId);
        }

        #region Message Handlers

        public void Handle(MoneyAddedToAccount message)
        {
            Data.CustomerId = message.CustomerId;
            Data.Balance += message.Amount;

            Console.WriteLine("------------------");
            Console.WriteLine("Money Deposited: " + message.Amount + " for customer " + message.CustomerId);
            PrintBalance();
            Console.WriteLine("------------------");
        }

        public void Handle(OrderAccepted order)
        {
            Data.CustomerId = order.CustomerId;
            // apply the discount
            var discount = CalculateDiscount();
            var amountAfterDiscount = order.Amount - (order.Amount * discount);

            // reject order if not enough money
            if (amountAfterDiscount > Data.Balance)
            {
                RejectOrder(order);
                return;
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

            Console.WriteLine("------------------");
            Console.WriteLine("Order Completed: " + order.OrderId + " " + order.CustomerId + " " + order.ProductId + " " + amountAfterDiscount);
            if (discount > 0)
            {
                Console.WriteLine("***Applied Discount of " + (discount * 100) +"% to " + order.Amount);
            }
            PrintBalance();
            Console.WriteLine("------------------");
        }
        #endregion

        #region Private Methods
        private double CalculateDiscount()
        {
            if (Data.Orders == null)
            {
                Data.Orders = new List<Order>();
            }

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

            Console.WriteLine("------------------");
            Console.WriteLine("Order Rejected: " + order.OrderId + " " + order.CustomerId + " " + order.ProductId + " " + order.Amount);
            Console.WriteLine("------------------");
        }

        private void PrintBalance()
        {
            Console.WriteLine("*** Balance is : " + Data.Balance);
        }

        #endregion
    }
}
