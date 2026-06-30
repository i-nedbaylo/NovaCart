using FluentAssertions;
using NovaCart.Services.Identity.Application.Commands;

namespace NovaCart.Tests.Identity.UnitTests;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("user@novacart.com", "Password1", "Ada", "Lovelace");

    [Fact]
    public void Validate_Should_Pass_For_A_Valid_Command()
    {
        _validator.Validate(ValidCommand()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_Should_Fail_For_Invalid_Email(string email)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_Should_Fail_For_Empty_Or_Too_Short_Password(string password)
    {
        var result = _validator.Validate(ValidCommand() with { Password = password });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Validate_Should_Fail_For_Empty_FirstName()
    {
        _validator.Validate(ValidCommand() with { FirstName = "" }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_For_Empty_LastName()
    {
        _validator.Validate(ValidCommand() with { LastName = "" }).IsValid.Should().BeFalse();
    }
}
