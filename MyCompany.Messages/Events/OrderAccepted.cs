namespace MyCompany.Messages.Events
{
    public interface OrderAccepted : IEvent
    {
        string OrderId { get; set; }
        string CustomerId { get; set; }
        string ProductId { get; set; }
        double Amount { get; set; }
    }
}
