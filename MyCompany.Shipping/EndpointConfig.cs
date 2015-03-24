using NServiceBus;

namespace MyCompany.Shipping
{
    class CustomizingHost : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.EndpointName("MyCompany.Shipping");
        }
    }
}
