using Microsoft.Extensions.Logging;
using OrderProcessing.Application.DTOs;
using OrderProcessing.Application.Interfaces;
using OrderProcessing.Application.Mappings;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Enums;
using OrderProcessing.Domain.Exceptions;

namespace OrderProcessing.Application.Services;

public sealed class OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<OrderService> _logger = logger;

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order for customer {CustomerEmail}", request.CustomerEmail);

        var items = request.Items.Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice));
        var order = Order.Create(request.CustomerName, request.CustomerEmail, items);

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerEmail}", order.Id, request.CustomerEmail);
        return order.ToResponse();
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        return order.ToResponse();
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllOrdersAsync(OrderStatus? status, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(status, cancellationToken);
        return orders.Select(o => o.ToResponse()).ToList().AsReadOnly();
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, request.Status);

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        order.UpdateStatus(request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, request.Status);
        return order.ToResponse();
    }

    public async Task CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling order {OrderId}", orderId);

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new OrderNotFoundException(orderId);

        order.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
    }

    public async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Background job: processing pending orders");

        var pendingOrders = await _unitOfWork.Orders.GetPendingOrdersAsync(cancellationToken);
        if (pendingOrders.Count == 0)
        {
            _logger.LogInformation("Background job: no pending orders found");
            return;
        }

        foreach (var order in pendingOrders)
        {
            order.AdvanceToProcessing();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Background job: {Count} order(s) advanced to PROCESSING", pendingOrders.Count);
    }
}
