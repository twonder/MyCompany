using NServiceBus;
using NServiceBus.Persistence;

namespace MyCompany.Orders
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            // using json instead of XML
            configuration.UseSerialization<JsonSerializer>();

            // does not work with RavenDB 3+, make sure to download 2.5*
            // persisting sagas and timeouts to RavenDB
            configuration.UsePersistence<RavenDBPersistence>().DoNotSetupDatabasePermissions();

            // specify what the commands and events can be recognized by
            configuration.Conventions().DefiningCommandsAs(e => e.Namespace != null & e.Namespace.EndsWith("Commands"));
            configuration.Conventions().DefiningEventsAs(e => e.Namespace != null & e.Namespace.EndsWith("Events"));
        }
    }
}
