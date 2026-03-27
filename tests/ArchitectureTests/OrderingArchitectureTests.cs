using NetArchTest.Rules;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Application.Commands;
using OrderingInfra = NovaCart.Services.Ordering.Infrastructure;

namespace NovaCart.Tests.ArchitectureTests;

public class OrderingArchitectureTests
{
    private const string ApplicationNamespace = "NovaCart.Services.Ordering.Application";
    private const string InfrastructureNamespace = "NovaCart.Services.Ordering.Infrastructure";
    private const string ApiNamespace = "NovaCart.Services.Ordering.API";

    [Fact]
    public void Domain_Should_NotDependOn_Application()
    {
        var result = Types
            .InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Ordering.Domain has forbidden dependency on Application: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Ordering.Domain has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Ordering.Domain has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(CreateOrderCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Ordering.Application has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(CreateOrderCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Ordering.Application has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Infrastructure_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(OrderingInfra.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Ordering.Infrastructure has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
            return "none";

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
