using FluentValidation;
using OrderProcessing.Application.DTOs;
using OrderProcessing.Domain.Enums;

namespace OrderProcessing.Application.Validators;

public sealed class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    private static readonly IReadOnlySet<OrderStatus> AllowedTransitions = new HashSet<OrderStatus>
    {
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Delivered
    };

    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => AllowedTransitions.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedTransitions.Select(s => s.ToString().ToUpperInvariant()))}. Use the cancel endpoint to cancel an order.");
    }
}
