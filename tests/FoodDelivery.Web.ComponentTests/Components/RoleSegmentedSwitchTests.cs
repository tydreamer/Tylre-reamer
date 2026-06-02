using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class RoleSegmentedSwitchTests : ComponentTestBase
{
    [Fact]
    public void Renders_CustomerAsActive_WhenValueIsCustomer()
    {
        var cut = Render<RoleSegmentedSwitch>(ps => ps.Add(p => p.Value, "Customer"));

        var customerButton = cut.FindAll("button").First(b => b.TextContent.Contains("Customer"));
        customerButton.ClassList.Should().Contain("active");
        customerButton.HasAttribute("aria-pressed").Should().BeTrue();
    }

    [Fact]
    public async Task ClickOwner_InvokesValueChanged()
    {
        string? newValue = null;

        var cut = Render<RoleSegmentedSwitch>(ps => ps
            .Add(p => p.Value, "Customer")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, role =>
            {
                newValue = role;
                return Task.CompletedTask;
            })));

        await cut.FindAll("button").First(b => b.TextContent.Contains("Owner")).ClickAsync();

        newValue.Should().Be("Owner");
    }

    [Fact]
    public async Task ClickCustomer_WhenAlreadyCustomer_DoesNotInvokeValueChanged()
    {
        var invokeCount = 0;

        var cut = Render<RoleSegmentedSwitch>(ps => ps
            .Add(p => p.Value, "Customer")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, _ =>
            {
                invokeCount++;
                return Task.CompletedTask;
            })));

        await cut.FindAll("button").First(b => b.TextContent.Contains("Customer")).ClickAsync();

        invokeCount.Should().Be(0);
    }
}
