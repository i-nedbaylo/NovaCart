using NetArchTest.Rules;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Infrastructure;

namespace NovaCart.Tests.ArchitectureTests;

public class BasketArchitectureTests
{
    private const string ApplicationNamespace = "NovaCart.Services.Basket.Application";
    private const string InfrastructureNamespace = "NovaCart.Services.Basket.Infrastructure";
    private const string ApiNamespace = "NovaCart.Services.Basket.API";

    [Fact]
    public void Domain_Should_NotDependOn_Application()
    {
        var result = Types
            .InAssembly(typeof(ShoppingCart).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Basket.Domain has forbidden dependency on Application: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(ShoppingCart).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Basket.Domain has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(ShoppingCart).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Basket.Domain has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(CheckoutBasketCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Basket.Application has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(CheckoutBasketCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Basket.Application has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Infrastructure_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Basket.Infrastructure has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
            return "none";

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
