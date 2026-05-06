using System.Reflection;
using NetArchTest.Rules;
using OrderProcessing.API.Controllers;
using OrderProcessing.Application.Services;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Infrastructure.Persistence;

namespace OrderProcessing.ArchitectureTests;

/// <summary>
/// Holds the assembly references shared across all architecture test classes.
/// </summary>
internal static class Assemblies
{
    public static readonly Assembly Domain         = typeof(Order).Assembly;
    public static readonly Assembly Application    = typeof(OrderService).Assembly;
    public static readonly Assembly Infrastructure = typeof(AppDbContext).Assembly;
    public static readonly Assembly Api            = typeof(OrdersController).Assembly;
}
