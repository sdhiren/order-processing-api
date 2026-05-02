using Microsoft.EntityFrameworkCore;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Enums;

namespace OrderProcessing.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Orders.AnyAsync())
            return;

        var orders = new List<Order>
        {
            CreateOrder(
                "Alice Johnson", "alice@example.com",
                [("Mechanical Keyboard", 1, 129.99m), ("USB-C Hub", 2, 39.99m)],
                OrderStatus.Pending),

            CreateOrder(
                "Bob Martinez", "bob@example.com",
                [("Standing Desk", 1, 499.00m)],
                OrderStatus.Processing),

            CreateOrder(
                "Carol White", "carol@example.com",
                [("Monitor 27\"", 1, 349.00m), ("Monitor Arm", 1, 59.99m)],
                OrderStatus.Shipped),

            CreateOrder(
                "David Lee", "david@example.com",
                [("Laptop Backpack", 1, 79.99m), ("Wireless Mouse", 1, 49.99m), ("Mousepad XL", 1, 24.99m)],
                OrderStatus.Delivered),

            CreateOrder(
                "Eva Chen", "eva@example.com",
                [("Webcam 4K", 1, 199.99m)],
                OrderStatus.Cancelled),

            CreateOrder(
                "Frank Brown", "frank@example.com",
                [("Noise-Cancelling Headphones", 1, 299.99m), ("Headphone Stand", 1, 34.99m)],
                OrderStatus.Pending),

            CreateOrder(
                "Grace Kim", "grace@example.com",
                [("Ergonomic Chair", 1, 649.00m)],
                OrderStatus.Processing),
        };

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }

    private static Order CreateOrder(
        string customerName,
        string customerEmail,
        (string ProductName, int Quantity, decimal UnitPrice)[] items,
        OrderStatus targetStatus)
    {
        var orderItems = items.Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice));
        var order = Order.Create(customerName, customerEmail, orderItems);

        if (targetStatus != OrderStatus.Pending && targetStatus != OrderStatus.Cancelled)
            order.UpdateStatus(targetStatus);

        if (targetStatus == OrderStatus.Cancelled)
            order.Cancel();

        return order;
    }
}
