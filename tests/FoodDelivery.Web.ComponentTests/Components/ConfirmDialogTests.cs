using Bunit;
using FluentAssertions;
using FoodDelivery.Web.Components;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class ConfirmDialogTests : ComponentTestBase
{
    [Fact]
    public void DoesNotRender_WhenNotVisible()
    {
        var cut = Render<ConfirmDialog>(ps => ps.Add(p => p.IsVisible, false));

        cut.Markup.Should().NotContain("modal-dialog");
    }

    [Fact]
    public void Renders_TitleAndMessage_WhenVisible()
    {
        var cut = Render<ConfirmDialog>(ps => ps
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Remove meal")
            .Add(p => p.Message, "Remove \"Pasta\"? This cannot be undone."));

        cut.Find(".modal-title").TextContent.Should().Be("Remove meal");
        cut.Find(".modal-body").TextContent.Should().Contain("Remove \"Pasta\"?");
    }

    [Fact]
    public async Task Confirm_InvokesOnConfirmed_AndClosesDialog()
    {
        var visible = true;
        var confirmed = false;

        var cut = Render<ConfirmDialog>(ps => ps
            .Add(p => p.IsVisible, visible)
            .Add(p => p.IsVisibleChanged, EventCallback.Factory.Create<bool>(this, v => visible = v))
            .Add(p => p.ConfirmText, "Remove")
            .Add(p => p.OnConfirmed, EventCallback.Factory.Create(this, () =>
            {
                confirmed = true;
                return Task.CompletedTask;
            })));

        await cut.FindAll("button").First(b => b.TextContent.Contains("Remove")).ClickAsync();

        confirmed.Should().BeTrue();
        visible.Should().BeFalse();
        cut.Markup.Should().NotContain("modal-dialog");
    }

    [Fact]
    public async Task Cancel_ClosesWithoutInvokingOnConfirmed()
    {
        var visible = true;
        var confirmed = false;

        var cut = Render<ConfirmDialog>(ps => ps
            .Add(p => p.IsVisible, visible)
            .Add(p => p.IsVisibleChanged, EventCallback.Factory.Create<bool>(this, v => visible = v))
            .Add(p => p.CancelText, "Cancel")
            .Add(p => p.OnConfirmed, EventCallback.Factory.Create(this, () =>
            {
                confirmed = true;
                return Task.CompletedTask;
            })));

        await cut.FindAll("button").First(b => b.TextContent.Contains("Cancel")).ClickAsync();

        confirmed.Should().BeFalse();
        visible.Should().BeFalse();
    }
}
