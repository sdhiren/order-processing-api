using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderProcessing.Application.DTOs;
using OrderProcessing.Application.Interfaces;
using OrderProcessing.Application.Services;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Enums;
using OrderProcessing.Domain.Exceptions;

namespace OrderProcessing.UnitTests.Services;

public sealed class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _uowMock.Setup(u => u.Orders).Returns(_repositoryMock.Object);
        _sut = new OrderService(_uowMock.Object, NullLogger<OrderService>.Instance);
    }

    private static Order BuildOrder(OrderStatus status = OrderStatus.Pending)
    {
        var item = new OrderItem("Widget", 1, 10m);
        var order = Order.Create("Alice", "alice@example.com", [item]);
        if (status != OrderStatus.Pending)
            order.UpdateStatus(status);
        return order;
    }

    // ---- CreateOrder ----

    [Fact]
    public async Task CreateOrderAsync_ShouldAddAndSaveOrder_AndReturnResponse()
    {
        var request = new CreateOrderRequest(
            "Alice",
            "alice@example.com",
            [new CreateOrderItemRequest("Widget", 2, 5m)]
        );

        var result = await _sut.CreateOrderAsync(request);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);

        result.CustomerEmail.Should().Be("alice@example.com");
        result.Status.Should().Be(OrderStatus.Pending);
        result.TotalAmount.Should().Be(10m);
        result.Items.Should().HaveCount(1);
    }

    // ---- GetOrderById ----

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderExists_ShouldReturnResponse()
    {
        var order = BuildOrder();
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.GetOrderByIdAsync(order.Id);

        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderNotFound_ShouldThrowOrderNotFoundException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Order?)null);

        var act = () => _sut.GetOrderByIdAsync(id);

        await act.Should().ThrowAsync<OrderNotFoundException>()
            .Where(e => e.Code == "ORDER_NOT_FOUND");
    }

    // ---- GetAllOrders ----

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnMappedOrders()
    {
        var orders = new List<Order> { BuildOrder(), BuildOrder() };
        _repositoryMock.Setup(r => r.GetAllAsync(null, default)).ReturnsAsync(orders);

        var result = await _sut.GetAllOrdersAsync(null);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllOrdersAsync_FilteredByStatus_ShouldPassStatusToRepository()
    {
        _repositoryMock.Setup(r => r.GetAllAsync(OrderStatus.Processing, default))
            .ReturnsAsync([BuildOrder(OrderStatus.Processing)]);

        var result = await _sut.GetAllOrdersAsync(OrderStatus.Processing);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(OrderStatus.Processing);
        _repositoryMock.Verify(r => r.GetAllAsync(OrderStatus.Processing, default), Times.Once);
    }

    // ---- UpdateOrderStatus ----

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenOrderExists_ShouldUpdateAndSave()
    {
        var order = BuildOrder();
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.UpdateOrderStatusAsync(order.Id, new UpdateOrderStatusRequest(OrderStatus.Shipped));

        result.Status.Should().Be(OrderStatus.Shipped);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenOrderNotFound_ShouldThrow()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Order?)null);

        var act = () => _sut.UpdateOrderStatusAsync(id, new UpdateOrderStatusRequest(OrderStatus.Shipped));

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    // ---- CancelOrder ----

    [Fact]
    public async Task CancelOrderAsync_WhenPending_ShouldCancelAndSave()
    {
        var order = BuildOrder(OrderStatus.Pending);
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        await _sut.CancelOrderAsync(order.Id);

        order.Status.Should().Be(OrderStatus.Cancelled);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenNotPending_ShouldThrowInvalidOrderOperationException()
    {
        var order = BuildOrder(OrderStatus.Processing);
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        var act = () => _sut.CancelOrderAsync(order.Id);

        await act.Should().ThrowAsync<InvalidOrderOperationException>();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotFound_ShouldThrowOrderNotFoundException()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Order?)null);

        var act = () => _sut.CancelOrderAsync(id);

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    // ---- ProcessPendingOrders ----

    [Fact]
    public async Task ProcessPendingOrdersAsync_ShouldAdvanceAllPendingOrders()
    {
        var orders = new List<Order> { BuildOrder(), BuildOrder() };
        _repositoryMock.Setup(r => r.GetPendingOrdersAsync(default)).ReturnsAsync(orders);

        await _sut.ProcessPendingOrdersAsync();

        orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Processing));
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingOrdersAsync_WhenNoPendingOrders_ShouldNotSave()
    {
        _repositoryMock.Setup(r => r.GetPendingOrdersAsync(default)).ReturnsAsync([]);

        await _sut.ProcessPendingOrdersAsync();

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}
