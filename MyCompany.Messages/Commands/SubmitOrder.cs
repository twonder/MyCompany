namespace MyCompany.Messages.Commands
{
    public interface SubmitOrder : ICommand
    {
        string OrderId { get; set; }
        string CustomerId { get; set; }
        string ProductId { get; set; }
        double Amount { get; set; }
    }
}
