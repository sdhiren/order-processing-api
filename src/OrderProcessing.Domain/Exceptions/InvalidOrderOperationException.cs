namespace OrderProcessing.Domain.Exceptions;

public sealed class InvalidOrderOperationException : DomainException
{
    public InvalidOrderOperationException(string message)
        : base("INVALID_ORDER_OPERATION", message)
    {
    }
}
