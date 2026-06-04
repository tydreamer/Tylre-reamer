using Bunit;
using FluentAssertions;

namespace FoodDelivery.Web.ComponentTests.Components;

public class UserTypeBadgeTests : ComponentTestBase
{
    [Theory]
    [InlineData("Customer", "Regular User", "users-list-tag--customer")]
    [InlineData("Owner", "Restaurant Owner", "users-list-tag--owner")]
    [InlineData("Admin", "Administrator", "users-list-tag--admin")]
    public void Renders_CorrectLabelAndClass_ForRole(string role, string expectedLabel, string expectedClass)
    {
        var cut = Render<UserTypeBadge>(ps => ps.Add(p => p.Role, role));

        var span = cut.Find("span");
        span.TextContent.Should().Be(expectedLabel);
        span.ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void Renders_RoleValue_ForUnknownRole()
    {
        var cut = Render<UserTypeBadge>(ps => ps.Add(p => p.Role, "Moderator"));

        cut.Find("span").TextContent.Should().Be("Moderator");
    }
}
