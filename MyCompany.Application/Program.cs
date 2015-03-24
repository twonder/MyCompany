using System;
using System.Linq;
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
            busConfiguration.UseSerialization<XmlSerializer>();
            busConfiguration.UsePersistence<RavenDBPersistence>();
            busConfiguration.Conventions().DefiningCommandsAs(e => e.Namespace != null & e.Namespace.EndsWith("Messages"));
            busConfiguration.Conventions().DefiningEventsAs(e => e.Namespace != null & e.Namespace.EndsWith("Events"));
            var startableBus = Bus.Create(busConfiguration);
            using (var bus = startableBus.Start())
            {
                Start(bus);
            }
        }

        static void Start(IBus bus)
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
                            bus.Publish<OrderAccepted>(o =>
                            {
                                o.DateOccurred = DateTime.Now;
                                o.OrderId = orderId;
                                o.CustomerId = customerId;
                                o.ProductId = pieces[1];
                                o.Amount = Convert.ToDouble(pieces[2]);
                            });

                            Console.WriteLine("===> Order Accepted");

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
                        default:

                            continue;
                            break;
                    }
                }
                catch (Exception e)
                {
                    continue;
                }

                Console.WriteLine("==========================================================================");
            }
        }

        static void PrintInstructions()
        {
            Console.WriteLine("Here are the list of actions you can run:");
            Console.WriteLine("Login:customerId");
            Console.WriteLine("CreateOrder:productId:amount");
            Console.WriteLine("DepositMoney:amount");
        }
    }
}

