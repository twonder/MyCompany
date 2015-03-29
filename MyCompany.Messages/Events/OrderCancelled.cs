namespace MyCompany.Messages.Events
{
    public interface OrderCancelled : IEvent
    {
        string OrderId { get; set; }
        string CustomerId { get; set; }
    }
}