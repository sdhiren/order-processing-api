namespace OrderProcessing.Domain.Exceptions;

public sealed class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(Guid orderId)
        : base("ORDER_NOT_FOUND", $"Order with ID '{orderId}' was not found.")
    {
    }
}
