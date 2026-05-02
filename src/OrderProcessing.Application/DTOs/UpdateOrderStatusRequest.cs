using OrderProcessing.Domain.Enums;

namespace OrderProcessing.Application.DTOs;

public sealed record UpdateOrderStatusRequest(OrderStatus Status);
