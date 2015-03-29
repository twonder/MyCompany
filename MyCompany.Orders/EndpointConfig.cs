using NServiceBus;
using NServiceBus.Persistence;

namespace MyCompany.Orders
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UseSerialization<XmlSerializer>();

            configuration.UsePersistence<NHibernatePersistence, StorageType.Sagas>();
            configuration.UsePersistence<NHibernatePersistence, StorageType.Subscriptions>();
            configuration.UsePersistence<NHibernatePersistence, StorageType.Timeouts>();
            configuration.UsePersistence<NHibernatePersistence, StorageType.Outbox>();
            configuration.UsePersistence<NHibernatePersistence, StorageType.GatewayDeduplication>();

            configuration.Conventions().DefiningCommandsAs(e => e.Namespace != null & e.Namespace.EndsWith("Messages"));
            configuration.Conventions().DefiningEventsAs(e => e.Namespace != null & e.Namespace.EndsWith("Events"));
        }
    }
}
