namespace MyCompany.Messages.Events
{
    public interface OrderRejected : IEvent
    {
        string OrderId { get; set; }
        string CustomerId { get; set; }
        string ProductId { get; set; }
    }
}
