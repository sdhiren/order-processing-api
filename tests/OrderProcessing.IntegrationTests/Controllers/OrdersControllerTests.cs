using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderProcessing.Application.DTOs;
using OrderProcessing.Domain.Enums;
using OrderProcessing.IntegrationTests.Fixtures;

namespace OrderProcessing.IntegrationTests.Controllers;

public sealed class OrdersControllerTests : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(OrderApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateOrderRequest SampleCreateRequest(string email = "test@example.com") => new(
        "Test Customer",
        email,
        [new CreateOrderItemRequest("Widget", 2, 15.00m)]
    );

    // ---- POST /api/orders ----

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturn201AndOrder()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", SampleCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(30m);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidRequest_ShouldReturn400()
    {
        var badRequest = new CreateOrderRequest("", "not-email", []);

        var response = await _client.PostAsJsonAsync("/api/orders", badRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- GET /api/orders/{id} ----

    [Fact]
    public async Task GetOrderById_WhenExists_ShouldReturn200()
    {
        var created = await CreateOrderAndGetResponse();

        var response = await _client.GetAsync($"/api/orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetOrderById_WhenNotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- GET /api/orders ----

    [Fact]
    public async Task GetAllOrders_ShouldReturn200WithOrders()
    {
        await CreateOrderAndGetResponse("list-test@example.com");

        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAllOrders_FilteredByStatus_ShouldReturnOnlyMatchingOrders()
    {
        await CreateOrderAndGetResponse("filter-test@example.com");

        var response = await _client.GetAsync("/api/orders?status=Pending");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }

    [Fact]
    public async Task GetAllOrders_FilteredByUppercaseStatus_ShouldReturn200WithMatchingOrders()
    {
        await CreateOrderAndGetResponse("uppercase-filter@example.com");

        var response = await _client.GetAsync("/api/orders?status=PENDING");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }

    [Fact]
    public async Task GetAllOrders_FilteredByLowercaseStatus_ShouldReturn200WithMatchingOrders()
    {
        await CreateOrderAndGetResponse("lowercase-filter@example.com");

        var response = await _client.GetAsync("/api/orders?status=pending");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }

    [Fact]
    public async Task GetAllOrders_FilteredByMixedCaseStatus_ShouldReturn200WithMatchingOrders()
    {
        await CreateOrderAndGetResponse("mixedcase-filter@example.com");

        var response = await _client.GetAsync("/api/orders?status=pEnDiNg");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }

    [Fact]
    public async Task GetAllOrders_FilteredByUnknownStatus_ShouldReturn400()
    {
        var response = await _client.GetAsync("/api/orders?status=UNKNOWN");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllOrders_FilteredByIntegerStatus_ShouldReturn400()
    {
        // After the change to string-only inputs, integer values (e.g. ?status=0)
        // must be rejected with 400 — they are no longer a valid contract.
        var response = await _client.GetAsync("/api/orders?status=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- PATCH /api/orders/{id}/status ----

    [Fact]
    public async Task UpdateOrderStatus_WhenValid_ShouldReturn200WithUpdatedStatus()
    {
        var created = await CreateOrderAndGetResponse();

        var response = await _client.PatchAsJsonAsync(
            $"/api/orders/{created.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Shipped));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithInvalidStatus_ShouldReturn400()
    {
        var created = await CreateOrderAndGetResponse();

        var response = await _client.PatchAsJsonAsync(
            $"/api/orders/{created.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Pending));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrderStatus_WhenOrderNotFound_ShouldReturn404()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/orders/{Guid.NewGuid()}/status",
            new UpdateOrderStatusRequest(OrderStatus.Shipped));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- DELETE /api/orders/{id} ----

    [Fact]
    public async Task CancelOrder_WhenPending_ShouldReturn204()
    {
        var created = await CreateOrderAndGetResponse();

        var response = await _client.DeleteAsync($"/api/orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelOrder_WhenNotPending_ShouldReturn422()
    {
        var created = await CreateOrderAndGetResponse();
        await _client.PatchAsJsonAsync(
            $"/api/orders/{created.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Processing));

        var response = await _client.DeleteAsync($"/api/orders/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CancelOrder_WhenNotFound_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- Health check ----

    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ---- Helper ----

    private async Task<OrderResponse> CreateOrderAndGetResponse(string email = "helper@example.com")
    {
        var response = await _client.PostAsJsonAsync("/api/orders", SampleCreateRequest(email));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }
}
