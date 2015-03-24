namespace MyCompany.Messages.Events
{
    public interface MoneyAddedToAccount : IEvent
    {
        string CustomerId { get; set; }
        double Amount { get; set; }
    }
}
