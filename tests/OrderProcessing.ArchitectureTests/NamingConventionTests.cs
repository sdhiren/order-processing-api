using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;

namespace OrderProcessing.ArchitectureTests;

/// <summary>
/// Verifies that naming conventions are consistently applied across all layers.
/// </summary>
public sealed class NamingConventionTests
{
    // ── Interfaces ────────────────────────────────────────────────────────────

    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        System.Reflection.Assembly[] allAssemblies =
        [
            Assemblies.Domain,
            Assemblies.Application,
            Assemblies.Infrastructure,
            Assemblies.Api
        ];

        var result = Types.InAssemblies(allAssemblies)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all interfaces must follow the 'I' prefix naming convention");
    }

    // ── Controllers ───────────────────────────────────────────────────────────

    [Fact]
    public void Controllers_Should_End_With_Controller()
    {
        var result = Types.InAssembly(Assemblies.Api)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all controller classes must end with 'Controller'");
    }

    // ── Services ──────────────────────────────────────────────────────────────

    [Fact]
    public void Service_Implementations_Should_End_With_Service()
    {
        var result = Types.InAssembly(Assemblies.Application)
            .That()
            .ImplementInterface(typeof(Application.Interfaces.IOrderService))
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all service implementation classes must end with 'Service'");
    }

    // ── Repositories ─────────────────────────────────────────────────────────

    [Fact]
    public void Repository_Implementations_Should_End_With_Repository()
    {
        var result = Types.InAssembly(Assemblies.Infrastructure)
            .That()
            .ImplementInterface(typeof(Application.Interfaces.IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all repository implementation classes must end with 'Repository'");
    }

    // ── Exceptions ────────────────────────────────────────────────────────────

    [Fact]
    public void Exceptions_Should_End_With_Exception()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all exception classes must end with 'Exception'");
    }

    // ── Validators ────────────────────────────────────────────────────────────

    [Fact]
    public void Validators_Should_End_With_Validator()
    {
        // Any type placed in the Validators namespace must follow the naming convention.
        // This prevents names like 'OrderCheck' living alongside properly named validators.
        var result = Types.InAssembly(Assemblies.Application)
            .That()
            .ResideInNamespace("OrderProcessing.Application.Validators")
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "all types in the Validators namespace must end with 'Validator'");
    }
}
