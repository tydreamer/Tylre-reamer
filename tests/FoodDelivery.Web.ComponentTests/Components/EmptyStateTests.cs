using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class EmptyStateTests : ComponentTestBase
{
    [Fact]
    public void Renders_Message()
    {
        var cut = Render<EmptyState>(ps => ps
            .Add(p => p.Message, "No items found."));

        cut.Find(".empty-state-message").TextContent.Should().Be("No items found.");
    }

    [Fact]
    public void Renders_Title_WhenProvided()
    {
        var cut = Render<EmptyState>(ps => ps
            .Add(p => p.Title, "Nothing here")
            .Add(p => p.Message, "Try adding something."));

        cut.Find(".empty-state-title").TextContent.Should().Be("Nothing here");
    }

    [Fact]
    public void DoesNotRender_Title_WhenNotProvided()
    {
        var cut = Render<EmptyState>(ps => ps
            .Add(p => p.Message, "No items."));

        cut.FindAll(".empty-state-title").Should().BeEmpty();
    }

    [Fact]
    public void Renders_ChildContent_WhenProvided()
    {
        var cut = Render<EmptyState>(ps => ps
            .Add(p => p.Message, "No items.")
            .Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenElement(0, "a");
                b.AddAttribute(1, "href", "/add");
                b.AddContent(2, "Add item");
                b.CloseElement();
            })));

        cut.Find(".empty-state-actions a").TextContent.Should().Be("Add item");
    }

    [Fact]
    public void DoesNotRender_Actions_WhenNoChildContent()
    {
        var cut = Render<EmptyState>(ps => ps
            .Add(p => p.Message, "No items."));

        cut.FindAll(".empty-state-actions").Should().BeEmpty();
    }

    [Fact]
    public void Renders_CustomIcon()
    {
        var cut = Render<EmptyState>(ps => ps
            .Add(p => p.Icon, "bi bi-cart")
            .Add(p => p.Message, "Cart is empty."));

        cut.Find("i").ClassList.Should().Contain("bi-cart");
    }
}
