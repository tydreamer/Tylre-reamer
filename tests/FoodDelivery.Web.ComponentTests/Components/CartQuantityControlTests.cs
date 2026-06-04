using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class CartQuantityControlTests : ComponentTestBase
{
    [Fact]
    public void Renders_Quantity()
    {
        var cut = Render<CartQuantityControl>(ps => ps
            .Add(p => p.Quantity, 3)
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { })));

        cut.Find(".cart-qty-display").TextContent.Should().Be("3");
    }

    [Fact]
    public void DecreaseButton_IsDisabled_WhenDecreaseDisabledIsTrue()
    {
        var cut = Render<CartQuantityControl>(ps => ps
            .Add(p => p.Quantity, 1)
            .Add(p => p.DecreaseDisabled, true)
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { })));

        var decreaseBtn = cut.FindAll("button").First(b => b.TextContent.Contains("−"));
        decreaseBtn.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void DecreaseButton_IsEnabled_WhenDecreaseDisabledIsFalse()
    {
        var cut = Render<CartQuantityControl>(ps => ps
            .Add(p => p.Quantity, 2)
            .Add(p => p.DecreaseDisabled, false)
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { })));

        var decreaseBtn = cut.FindAll("button").First(b => b.TextContent.Contains("−"));
        decreaseBtn.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public async Task ClickIncrease_InvokesOnIncrease()
    {
        var increased = false;

        var cut = Render<CartQuantityControl>(ps => ps
            .Add(p => p.Quantity, 1)
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { increased = true; })));

        await cut.FindAll("button").First(b => b.TextContent.Contains("+")).ClickAsync();

        increased.Should().BeTrue();
    }

    [Fact]
    public async Task ClickDecrease_InvokesOnDecrease()
    {
        var decreased = false;

        var cut = Render<CartQuantityControl>(ps => ps
            .Add(p => p.Quantity, 2)
            .Add(p => p.DecreaseDisabled, false)
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { decreased = true; }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { })));

        await cut.FindAll("button").First(b => b.TextContent.Contains("−")).ClickAsync();

        decreased.Should().BeTrue();
    }
}
