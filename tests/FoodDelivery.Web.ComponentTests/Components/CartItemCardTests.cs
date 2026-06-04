using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class CartItemCardTests : ComponentTestBase
{
    private static CartItem MakeItem(string name = "Burger", decimal price = 10m, int quantity = 2) =>
        new(new MealDto(1, name, "", "", price, 1, 1, "Main"), quantity);

    [Fact]
    public void Renders_MealName()
    {
        var cut = Render<CartItemCard>(ps => ps
            .Add(p => p.Item, MakeItem("Pasta"))
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRemove, EventCallback.Factory.Create(this, () => { })));

        cut.Find(".card-title").TextContent.Should().Be("Pasta");
    }

    [Fact]
    public void Renders_LineTotal()
    {
        var cut = Render<CartItemCard>(ps => ps
            .Add(p => p.Item, MakeItem(price: 10m, quantity: 3))
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRemove, EventCallback.Factory.Create(this, () => { })));

        cut.Find(".cart-item-total").TextContent.Should().Contain("30");
    }

    [Fact]
    public void DecreaseDisabled_WhenQuantityIsOne()
    {
        var cut = Render<CartItemCard>(ps => ps
            .Add(p => p.Item, MakeItem(quantity: 1))
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRemove, EventCallback.Factory.Create(this, () => { })));

        var decreaseBtn = cut.FindAll("button").First(b => b.TextContent.Contains("−"));
        decreaseBtn.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task ClickRemove_InvokesOnRemove()
    {
        var removed = false;

        var cut = Render<CartItemCard>(ps => ps
            .Add(p => p.Item, MakeItem())
            .Add(p => p.OnDecrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnIncrease, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnRemove, EventCallback.Factory.Create(this, () => { removed = true; })));

        await cut.Find(".cart-remove-btn").ClickAsync();

        removed.Should().BeTrue();
    }
}
