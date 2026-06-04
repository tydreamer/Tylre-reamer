using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class BrowseSearchBarTests : ComponentTestBase
{
    [Fact]
    public void Renders_WithDefaultPlaceholder()
    {
        var cut = Render<BrowseSearchBar>();

        cut.Find("input").GetAttribute("placeholder").Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Renders_WithCustomPlaceholder()
    {
        var cut = Render<BrowseSearchBar>(ps => ps
            .Add(p => p.Placeholder, "Search meals..."));

        cut.Find("input").GetAttribute("placeholder").Should().Be("Search meals...");
    }

    [Fact]
    public void Renders_InitialSearchTerm()
    {
        var cut = Render<BrowseSearchBar>(ps => ps
            .Add(p => p.SearchTerm, "pizza"));

        cut.Find("input").GetAttribute("value").Should().Be("pizza");
    }

    [Fact]
    public async Task EnterKey_TriggersOnSearch_Immediately()
    {
        var searched = false;

        var cut = Render<BrowseSearchBar>(ps => ps
            .Add(p => p.OnSearch, EventCallback.Factory.Create(this, () => { searched = true; })));

        await cut.Find("input").KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs
        {
            Key = "Enter"
        });

        searched.Should().BeTrue();
    }

    [Fact]
    public async Task Input_UpdatesSearchTerm_AndNotifiesChanged()
    {
        var captured = "";

        var cut = Render<BrowseSearchBar>(ps => ps
            .Add(p => p.SearchTermChanged, EventCallback.Factory.Create<string>(this, v => captured = v)));

        await cut.Find("input").InputAsync(new Microsoft.AspNetCore.Components.Web.ChangeEventArgs
        {
            Value = "burger"
        });

        captured.Should().Be("burger");
    }
}
