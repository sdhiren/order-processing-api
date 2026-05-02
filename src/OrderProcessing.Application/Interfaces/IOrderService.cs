using OrderProcessing.Application.DTOs;
using OrderProcessing.Domain.Enums;

namespace OrderProcessing.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponse>> GetAllOrdersAsync(OrderStatus? status, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task ProcessPendingOrdersAsync(CancellationToken cancellationToken = default);
}
