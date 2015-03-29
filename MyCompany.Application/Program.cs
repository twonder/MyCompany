using System;
using System.Linq;
using MyCompany.Messages.Commands;
using MyCompany.Messages.Events;
using NServiceBus;
using NServiceBus.Persistence;

namespace MyCompany.Application
{
    static class Program
    {
        private static string customerId;
        static void Main()
        {
            var busConfiguration = new BusConfiguration();

            // using json instead of XML
            busConfiguration.UseSerialization<JsonSerializer>();

            // persisting subscriptions to RavenDB
            busConfiguration.UsePersistence<RavenDBPersistence>();

            // specify what the commands and events can be recognized by
            busConfiguration.Conventions().DefiningCommandsAs(e => e.Namespace != null & e.Namespace.EndsWith("Commands"));
            busConfiguration.Conventions().DefiningEventsAs(e => e.Namespace != null & e.Namespace.EndsWith("Events"));

            var startableBus = Bus.Create(busConfiguration);
            using (var bus = startableBus.Start())
            {
                RunApplication(bus);
            }
        }

        static void RunApplication(IBus bus)
        {
            PrintInstructions();
            var line = "";
            while (line != null)
            {
                line = Console.ReadLine();

                try
                {
                    var pieces = line.Split(':');
                    var actionEntered = pieces[0];

                    switch (actionEntered)
                    {
                        case "Login":
                            customerId = pieces[1];

                            Console.WriteLine("===> Logged in");

                            break;
                        case "CreateOrder":
                            var orderId = Guid.NewGuid().ToString();
                            bus.Send<SubmitOrder>(o =>
                            {
                                o.DateSent = DateTime.Now;
                                o.OrderId = orderId;
                                o.CustomerId = customerId;
                                o.ProductId = pieces[1];
                                o.Amount = Convert.ToDouble(pieces[2]);
                            });

                            Console.WriteLine("===> Order Submitted: " + orderId);

                            break;
                        case "DepositMoney":
                            var temp = customerId;

                            bus.Publish<MoneyAddedToAccount>(m =>
                            {
                                m.DateOccurred = DateTime.Now;
                                m.CustomerId = customerId;
                                m.Amount = Convert.ToDouble(pieces[1]);
                            });

                            Console.WriteLine("===> Money Deposited");

                            break;
                        case "CancelOrder":

                            bus.Publish<OrderCancelled>(m =>
                            {
                                m.DateOccurred = DateTime.Now;
                                m.OrderId = pieces[1];
                                m.CustomerId = customerId;
                            });

                            Console.WriteLine("===> Order Cancelled");

                            break;
                        default:
                            Console.WriteLine("===> Unrecognized Action");
                            continue;
                            break;
                    }
                }
                catch (Exception e)
                {
                    continue;
                }

                Console.WriteLine("==========================================================================");
                PrintInstructions();
            }
        }

        static void PrintInstructions()
        {
            Console.WriteLine("Here are the list of actions you can run:");
            Console.WriteLine("Login:customerId");
            Console.WriteLine("CreateOrder:productId:amount (assumes prior login)");
            Console.WriteLine("DepositMoney:amount (assumes prior login)");
            Console.WriteLine("CancelOrder:orderId");
            Console.WriteLine("==========================================================================");
            Console.Write((customerId ?? "NotLoggedIn") + "> ");
        }
    }
}

