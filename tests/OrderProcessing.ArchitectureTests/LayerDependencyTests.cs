using FluentAssertions;
using NetArchTest.Rules;

namespace OrderProcessing.ArchitectureTests;

/// <summary>
/// Enforces Clean Architecture dependency rules between layers.
///
/// Allowed dependency graph:
///   Domain        ← (no dependencies)
///   Application   → Domain
///   Infrastructure → Domain, Application
///   API           → Domain, Application, Infrastructure
/// </summary>
public sealed class LayerDependencyTests
{
    // ── Domain ───────────────────────────────────────────────────────────────

    [Fact]
    public void Domain_Should_Not_DependOn_Application()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .Should()
            .NotHaveDependencyOn("OrderProcessing.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Domain layer must not reference the Application layer");
    }

    [Fact]
    public void Domain_Should_Not_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .Should()
            .NotHaveDependencyOn("OrderProcessing.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Domain layer must not reference the Infrastructure layer");
    }

    [Fact]
    public void Domain_Should_Not_DependOn_Api()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .Should()
            .NotHaveDependencyOn("OrderProcessing.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Domain layer must not reference the API layer");
    }

    // ── Application ──────────────────────────────────────────────────────────

    [Fact]
    public void Application_Should_Not_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(Assemblies.Application)
            .Should()
            .NotHaveDependencyOn("OrderProcessing.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Application layer must not reference the Infrastructure layer");
    }

    [Fact]
    public void Application_Should_Not_DependOn_Api()
    {
        var result = Types.InAssembly(Assemblies.Application)
            .Should()
            .NotHaveDependencyOn("OrderProcessing.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Application layer must not reference the API layer");
    }

    // ── Infrastructure ────────────────────────────────────────────────────────

    [Fact]
    public void Infrastructure_Should_Not_DependOn_Api()
    {
        var result = Types.InAssembly(Assemblies.Infrastructure)
            .Should()
            .NotHaveDependencyOn("OrderProcessing.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the Infrastructure layer must not reference the API layer");
    }

    // ── Controllers ───────────────────────────────────────────────────────────

    [Fact]
    public void Controllers_Should_Reside_Only_In_Api_Layer()
    {
        var nonApiControllers = Types
            .InAssemblies([Assemblies.Domain, Assemblies.Application, Assemblies.Infrastructure])
            .That()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        nonApiControllers.Should().BeEmpty(
            "controllers must only reside in the API layer");
    }

    [Fact]
    public void Controllers_Should_Not_Reference_Repositories_Directly()
    {
        var result = Types.InAssembly(Assemblies.Api)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .NotHaveDependencyOn("OrderProcessing.Infrastructure.Persistence.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "controllers must interact with repositories only via application-layer interfaces");
    }
}
