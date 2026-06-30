using FluentAssertions;
using NovaCart.Services.Identity.Application.Commands;

namespace NovaCart.Tests.Identity.UnitTests;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    private static LoginCommand ValidCommand() => new("user@novacart.com", "Password1");

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
        _validator.Validate(ValidCommand() with { Email = email }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_For_Empty_Password()
    {
        _validator.Validate(ValidCommand() with { Password = "" }).IsValid.Should().BeFalse();
    }
}
