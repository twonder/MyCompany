using System;
using MyCompany.Messages.Events;
using NServiceBus;

namespace MyCompany.Shipping.Backend
{
    public class HandleOrderCompleted : IHandleMessages<OrderCompleted>
    {
        public void Handle(OrderCompleted order)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==========================================================================");
            Console.WriteLine("Order Submitted: {0}.", order.DateOccurred);
            Console.WriteLine("Order Fullfilled: {0}.", DateTime.Now);

            Console.WriteLine("SENDING " + order.ProductId + " => " + order.CustomerId);
            Console.WriteLine("==========================================================================");
            Console.WriteLine(string.Empty);
        }
    }
}
