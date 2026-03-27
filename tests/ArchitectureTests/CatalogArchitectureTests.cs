using NetArchTest.Rules;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Application.Products.Commands;
using NovaCart.Services.Catalog.Infrastructure;

namespace NovaCart.Tests.ArchitectureTests;

public class CatalogArchitectureTests
{
    private const string DomainNamespace = "NovaCart.Services.Catalog.Domain";
    private const string ApplicationNamespace = "NovaCart.Services.Catalog.Application";
    private const string InfrastructureNamespace = "NovaCart.Services.Catalog.Infrastructure";
    private const string ApiNamespace = "NovaCart.Services.Catalog.API";

    [Fact]
    public void Domain_Should_NotDependOn_Application()
    {
        var result = Types
            .InAssembly(typeof(Product).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Catalog.Domain has forbidden dependency on Application: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(Product).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Catalog.Domain has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(Product).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Catalog.Domain has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(CreateProductCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Catalog.Application has forbidden dependency on Infrastructure: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_NotDependOn_Api()
    {
        var result = Types
            .InAssembly(typeof(CreateProductCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Catalog.Application has forbidden dependency on API: {FormatFailingTypes(result)}");
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
            $"Catalog.Infrastructure has forbidden dependency on API: {FormatFailingTypes(result)}");
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null || !result.FailingTypes.Any())
            return "none";

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
