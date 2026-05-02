using OrderProcessing.Domain.Enums;
using OrderProcessing.Domain.Exceptions;

namespace OrderProcessing.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerEmail { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

    // EF Core constructor
    private Order() { CustomerName = null!; CustomerEmail = null!; }

    public static Order Create(string customerName, string customerEmail, IEnumerable<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        order._items.AddRange(items);
        return order;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderOperationException(
                $"Order cannot be cancelled because its current status is '{Status}'. Only PENDING orders can be cancelled.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdvanceToProcessing()
    {
        if (Status == OrderStatus.Pending)
        {
            Status = OrderStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
