using NServiceBus;
using NServiceBus.Persistence;

namespace MyCompany.Orders
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UseSerialization<XmlSerializer>();

            // does not work with RavenDB 3+, make sure to download 2.5* 
            configuration.UsePersistence<RavenDBPersistence>().DoNotSetupDatabasePermissions();

            configuration.Conventions().DefiningCommandsAs(e => e.Namespace != null & e.Namespace.EndsWith("Messages"));
            configuration.Conventions().DefiningEventsAs(e => e.Namespace != null & e.Namespace.EndsWith("Events"));
        }
    }
}
