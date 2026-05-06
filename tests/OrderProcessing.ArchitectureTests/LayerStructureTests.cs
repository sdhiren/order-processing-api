using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;

namespace OrderProcessing.ArchitectureTests;

/// <summary>
/// Verifies structural rules specific to each architectural layer.
/// </summary>
public sealed class LayerStructureTests
{
    // ── Domain ────────────────────────────────────────────────────────────────

    [Fact]
    public void Domain_Entities_Should_Reside_In_Entities_Namespace()
    {
        // All types in Domain that are not exceptions or enums must live in the Entities namespace.
        // This catches helper or service classes accidentally placed at the wrong level.
        // Note: NetArchTest 1.3.x treats enum types as classes, so Enums must be excluded explicitly.
        var result = Types.InAssembly(Assemblies.Domain)
            .That()
            .AreClasses()
            .And()
            .DoNotResideInNamespace("OrderProcessing.Domain.Exceptions")
            .And()
            .DoNotResideInNamespace("OrderProcessing.Domain.Enums")
            .Should()
            .ResideInNamespace("OrderProcessing.Domain.Entities")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all non-exception, non-enum types in the Domain assembly must reside in 'OrderProcessing.Domain.Entities'");
    }

    [Fact]
    public void Domain_Exceptions_Should_Reside_In_Exceptions_Namespace()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .ResideInNamespace("OrderProcessing.Domain.Exceptions")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all domain exceptions must live under the 'OrderProcessing.Domain.Exceptions' namespace");
    }

    [Fact]
    public void Domain_Should_Not_Reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Domain layer must not have any EF Core dependency; persistence is an infrastructure concern");
    }

    [Fact]
    public void Domain_Should_Not_Reference_MicrosoftExtensionsDependencyInjection()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .Should()
            .NotHaveDependencyOn("Microsoft.Extensions.DependencyInjection")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Domain layer must not depend on the DI container");
    }

    // ── Application ──────────────────────────────────────────────────────────

    [Fact]
    public void Application_Service_Implementations_Should_Reside_In_Services_Namespace()
    {
        var result = Types.InAssembly(Assemblies.Application)
            .That()
            .ImplementInterface(typeof(Application.Interfaces.IOrderService))
            .Should()
            .ResideInNamespace("OrderProcessing.Application.Services")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "service implementations must reside in the 'OrderProcessing.Application.Services' namespace");
    }

    [Fact]
    public void Application_Interfaces_Should_Reside_In_Interfaces_Namespace()
    {
        var result = Types.InAssembly(Assemblies.Application)
            .That()
            .AreInterfaces()
            .Should()
            .ResideInNamespace("OrderProcessing.Application.Interfaces")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all application interfaces must live under the 'OrderProcessing.Application.Interfaces' namespace");
    }

    [Fact]
    public void Application_Should_Not_Reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(Assemblies.Application)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Application layer must not depend directly on EF Core; only Infrastructure may");
    }

    // ── Infrastructure ────────────────────────────────────────────────────────

    [Fact]
    public void Infrastructure_Repositories_Should_Reside_In_Repositories_Namespace()
    {
        var result = Types.InAssembly(Assemblies.Infrastructure)
            .That()
            .ImplementInterface(typeof(Application.Interfaces.IOrderRepository))
            .Should()
            .ResideInNamespace("OrderProcessing.Infrastructure.Persistence.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all repository implementations must reside in 'OrderProcessing.Infrastructure.Persistence.Repositories'");
    }

    // ── API ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Api_Controllers_Should_Reside_In_Controllers_Namespace()
    {
        var result = Types.InAssembly(Assemblies.Api)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .ResideInNamespace("OrderProcessing.API.Controllers")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all controllers must reside in the 'OrderProcessing.API.Controllers' namespace");
    }

    [Fact]
    public void Api_Controllers_Should_Be_Decorated_With_ApiController_Attribute()
    {
        var result = Types.InAssembly(Assemblies.Api)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .HaveCustomAttribute(typeof(ApiControllerAttribute))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all API controllers must be decorated with [ApiController]");
    }
}
