using System;
using System.Collections.Generic;
using System.Linq;
using MyCompany.Messages.Commands;
using MyCompany.Messages.Events;
using MyCompany.Orders.Data;
using MyCompany.Orders.Timeouts;
using NServiceBus;
using NServiceBus.Saga;
using Order = MyCompany.Orders.Data.Order;

namespace MyCompany.Orders
{
    public class PurchasePolicy : Saga<PurchasePolicyData>,
        IAmStartedByMessages<SubmitOrder>,
        IAmStartedByMessages<MoneyAddedToAccount>,
        IHandleMessages<OrderCancelled>,
        IHandleTimeouts<BuyersRemorseTimeout>
    {
        // the amount of money within a period of time that triggers a discount
        private double DiscountTotalThreshold = 100;

        // the amount of time in minutes to wait for an order cancellation
        private int MinutesToWaitForCancel = 1;

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PurchasePolicyData> mapper)
        {
            // Correlate incoming messages to the CustomerId of the saga
            mapper.ConfigureMapping<SubmitOrder>(order => order.CustomerId)
                .ToSaga(purchasePolicy => purchasePolicy.CustomerId);

            mapper.ConfigureMapping<MoneyAddedToAccount>(order => order.CustomerId)
                .ToSaga(purchasePolicy => purchasePolicy.CustomerId);

            mapper.ConfigureMapping<OrderCancelled>(order => order.CustomerId)
                .ToSaga(purchasePolicy => purchasePolicy.CustomerId);
        }

        // ----------------------- Message Handlers -----------------------
        public void Handle(MoneyAddedToAccount message)
        {
            Data.CustomerId = message.CustomerId;
            Data.Balance += message.Amount;

            Console.WriteLine("------------------");
            Console.WriteLine("Money Deposited: " + message.Amount + " for customer " + message.CustomerId);
            PrintBalance();
            Console.WriteLine("------------------");
        }

        public void Handle(SubmitOrder order)
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
                    Id = order.OrderId,
                    Date = DateTime.Now,
                    Amount = amountAfterDiscount
                });

            // call the timeout
            RequestTimeout(TimeSpan.FromMinutes(MinutesToWaitForCancel), new BuyersRemorseTimeout
            {
                CustomerId = order.CustomerId,
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                Amount = amountAfterDiscount,
                DiscountApplied = discount,
                DateOccurred = DateTime.Now
            });

            Console.WriteLine("------------------");
            Console.WriteLine("Order Accepted");
            Console.WriteLine(".... Waiting ....");
            Console.WriteLine("------------------");
        }

        // ----------------------- Timeouts -----------------------
        public void Timeout(BuyersRemorseTimeout state)
        {
            // if the order is no longer around, it was cancelled
            if (!Data.Orders.Any(o => o.Id == state.OrderId))
            {
                return;
            }

            // complete the order
            Bus.Publish<OrderCompleted>(o =>
            {
                o.CustomerId = state.CustomerId;
                o.OrderId = state.OrderId;
                o.ProductId = state.ProductId;
                o.Amount = state.Amount;
                o.DateOccurred = DateTime.Now;
            });

            Console.WriteLine("------------------");
            Console.WriteLine("Order Completed: " + state.OrderId + " " + state.CustomerId + " " + state.ProductId + " " + state.Amount + " after " + (state.DiscountApplied * 100) + "% discount");
            PrintBalance();
            Console.WriteLine("------------------");
        }


        public void Handle(OrderCancelled message)
        {
            // grab the order only if within the timeout length
            var order = Data.Orders.FirstOrDefault(o => o.Id == message.OrderId && message.DateOccurred.Subtract(o.Date) <= TimeSpan.FromMinutes(MinutesToWaitForCancel));

            // if no order found throw away cancellation (or talk to the business and see what they want to do with it)
            if (order == null)
            {
                Console.WriteLine("------------------");
                Console.WriteLine("Order already completed, can't be cancelled: " + message.OrderId);
                Console.WriteLine("------------------");
                return;
            }

            // deposit the amount back to the balance
            Data.Balance += order.Amount;

            // remove the order
            Data.Orders.Remove(order);

            Console.WriteLine("------------------");
            Console.WriteLine("Order Cancelled: " + message.OrderId);
            PrintBalance();
            Console.WriteLine("------------------");
        }

        // ----------------------- Private Methods -----------------------
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

        private void RejectOrder(SubmitOrder order)
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
    }
}
