using FluentAssertions;
using FoodDelivery.Web.Validation;

namespace FoodDelivery.Web.UnitTests.Validation;

public class PasswordStrengthEvaluatorTests
{
    [Fact]
    public void Evaluate_EmptyPassword_IsNotAcceptable()
    {
        var result = PasswordStrengthResult.Evaluate("");

        result.Level.Should().Be(PasswordStrengthLevel.Empty);
        result.IsAcceptable.Should().BeFalse();
        result.UnmetRequirements.Should().Contain("Enter a password.");
    }

    [Fact]
    public void Evaluate_WeakPassword_ListsUnmetRules()
    {
        var result = PasswordStrengthResult.Evaluate("short");

        result.Level.Should().Be(PasswordStrengthLevel.Weak);
        result.IsAcceptable.Should().BeFalse();
        result.UnmetRequirements.Should().Contain("At least 8 characters");
        result.UnmetRequirements.Should().Contain("One uppercase letter");
    }

    [Fact]
    public void Evaluate_StrongPassword_IsAcceptable()
    {
        var result = PasswordStrengthResult.Evaluate("Secure1!");

        result.Level.Should().Be(PasswordStrengthLevel.Strong);
        result.Score.Should().Be(5);
        result.IsAcceptable.Should().BeTrue();
        result.UnmetRequirements.Should().BeEmpty();
    }

    [Theory]
    [InlineData("longpass1", PasswordStrengthLevel.Fair)]
    [InlineData("NoDigits!Aa", PasswordStrengthLevel.Fair)]
    public void Evaluate_PartialRules_ReturnsFair(string password, PasswordStrengthLevel expectedLevel)
    {
        var result = PasswordStrengthResult.Evaluate(password);

        result.Level.Should().Be(expectedLevel);
        result.IsAcceptable.Should().BeFalse();
    }
}
