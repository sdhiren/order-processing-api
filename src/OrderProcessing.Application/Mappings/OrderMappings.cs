using OrderProcessing.Application.DTOs;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Enums;

namespace OrderProcessing.Application.Mappings;

internal static class OrderMappings
{
    internal static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerName,
            order.CustomerEmail,
            order.Status,
            order.Status.ToString().ToUpperInvariant(),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => i.ToResponse()).ToList().AsReadOnly()
        );
    }

    internal static OrderItemResponse ToResponse(this OrderItem item)
    {
        return new OrderItemResponse(
            item.Id,
            item.ProductName,
            item.Quantity,
            item.UnitPrice,
            item.TotalPrice
        );
    }
}
