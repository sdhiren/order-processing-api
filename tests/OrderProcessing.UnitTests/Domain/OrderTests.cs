using FluentAssertions;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Enums;
using OrderProcessing.Domain.Exceptions;

namespace OrderProcessing.UnitTests.Domain;

public sealed class OrderTests
{
    private static Order CreateSampleOrder()
    {
        var item = new OrderItem("Widget", 2, 9.99m);
        return Order.Create("John Doe", "john@example.com", [item]);
    }

    [Fact]
    public void Create_ShouldInitializeOrder_WithPendingStatus()
    {
        var order = CreateSampleOrder();

        order.Status.Should().Be(OrderStatus.Pending);
        order.CustomerName.Should().Be("John Doe");
        order.CustomerEmail.Should().Be("john@example.com");
        order.Items.Should().HaveCount(1);
        order.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void TotalAmount_ShouldSumAllItemTotals()
    {
        var items = new[] { new OrderItem("A", 2, 10m), new OrderItem("B", 3, 5m) };
        var order = Order.Create("Jane", "jane@example.com", items);

        order.TotalAmount.Should().Be(35m);
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetCancelledStatus()
    {
        var order = CreateSampleOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenNotPending_ShouldThrowInvalidOrderOperationException()
    {
        var order = CreateSampleOrder();
        order.UpdateStatus(OrderStatus.Processing);

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOrderOperationException>()
            .Which.Code.Should().Be("INVALID_ORDER_OPERATION");
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrowInvalidOrderOperationException()
    {
        var order = CreateSampleOrder();
        order.UpdateStatus(OrderStatus.Shipped);

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOrderOperationException>();
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus_AndUpdateTimestamp()
    {
        var order = CreateSampleOrder();
        var before = order.UpdatedAt;

        order.UpdateStatus(OrderStatus.Shipped);

        order.Status.Should().Be(OrderStatus.Shipped);
        order.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void AdvanceToProcessing_WhenPending_ShouldSetProcessingStatus()
    {
        var order = CreateSampleOrder();

        order.AdvanceToProcessing();

        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void AdvanceToProcessing_WhenNotPending_ShouldNotChangeStatus()
    {
        var order = CreateSampleOrder();
        order.UpdateStatus(OrderStatus.Shipped);

        order.AdvanceToProcessing();

        order.Status.Should().Be(OrderStatus.Shipped);
    }
}
