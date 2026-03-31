using NetArchTest.Rules;
using NovaCart.Services.Payment.Domain.Entities;
using NovaCart.Services.Payment.Application.Consumers;

namespace NovaCart.Tests.ArchitectureTests;

public class PaymentArchitectureTests
{
    private const string ApplicationNamespace = "NovaCart.Services.Payment.Application";
    private const string InfrastructureNamespace = "NovaCart.Services.Payment.Infrastructure";
    private const string ApiNamespace = "NovaCart.Services.Payment.API";

    [Fact]
    public void Domain_Should_NotDependOn_Application()
    {
        var result = Types
            .InAssembly(typeof(PaymentRecord).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Payment.Domain has forbidden dependency on Application: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(PaymentRecord).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Payment.Domain has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(PaymentRecord).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Payment.Domain has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(OrderCreatedIntegrationEventConsumer).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Payment.Application has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(OrderCreatedIntegrationEventConsumer).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Payment.Application has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null) return "none";
        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
