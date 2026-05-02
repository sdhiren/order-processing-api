using FluentAssertions;
using FluentValidation.TestHelper;
using OrderProcessing.Application.DTOs;
using OrderProcessing.Application.Validators;
using OrderProcessing.Domain.Enums;

namespace OrderProcessing.UnitTests.Validators;

public sealed class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    private static CreateOrderRequest ValidRequest() => new(
        "Jane Doe",
        "jane@example.com",
        [new CreateOrderItemRequest("Widget", 1, 9.99m)]
    );

    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_WhenCustomerNameEmpty_ShouldFail(string? name)
    {
        var req = ValidRequest() with { CustomerName = name! };
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public async Task Validate_WhenEmailInvalid_ShouldFail(string email)
    {
        var req = ValidRequest() with { CustomerEmail = email };
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Fact]
    public async Task Validate_WhenItemsEmpty_ShouldFail()
    {
        var req = ValidRequest() with { Items = [] };
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenItemQuantityNotPositive_ShouldFail(int qty)
    {
        var req = ValidRequest() with
        {
            Items = [new CreateOrderItemRequest("Widget", qty, 9.99m)]
        };
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WhenItemUnitPriceNotPositive_ShouldFail(double price)
    {
        var req = ValidRequest() with
        {
            Items = [new CreateOrderItemRequest("Widget", 1, (decimal)price)]
        };
        var result = await _validator.TestValidateAsync(req);
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}

public sealed class UpdateOrderStatusRequestValidatorTests
{
    private readonly UpdateOrderStatusRequestValidator _validator = new();

    [Theory]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public async Task Validate_WithAllowedStatus_ShouldPass(OrderStatus status)
    {
        var result = await _validator.TestValidateAsync(new UpdateOrderStatusRequest(status));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Validate_WithDisallowedStatus_ShouldFail(OrderStatus status)
    {
        var result = await _validator.TestValidateAsync(new UpdateOrderStatusRequest(status));
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
