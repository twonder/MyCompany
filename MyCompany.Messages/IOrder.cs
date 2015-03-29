namespace MyCompany.Messages
{
    public interface IOrder
    {
        string OrderId { get; set; }
        string CustomerId { get; set; }
        string ProductId { get; set; }
        double Amount { get; set; }
    }
}
