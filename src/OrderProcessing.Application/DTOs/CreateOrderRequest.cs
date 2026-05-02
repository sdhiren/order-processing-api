namespace OrderProcessing.Application.DTOs;

public sealed record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public sealed record CreateOrderRequest(
    string CustomerName,
    string CustomerEmail,
    IReadOnlyList<CreateOrderItemRequest> Items
);
