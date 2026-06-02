using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class SortableTableHeaderTests : ComponentTestBase
{
    [Fact]
    public void Renders_Label_AndSortIcon_WhenColumnIsActive()
    {
        var cut = Render<SortableTableHeader>(ps => ps
            .Add(p => p.ColumnKey, "date")
            .Add(p => p.Label, "Order Date")
            .Add(p => p.CurrentSortBy, "date")
            .Add(p => p.SortDesc, true));

        cut.Find("button").TextContent.Should().Contain("Order Date");
        cut.Find(".sortable-th-icon").TextContent.Should().Be("▼");
        cut.Find("button").ClassList.Should().Contain("sortable-th--active");
    }

    [Fact]
    public async Task Click_WhenActive_TogglesSortDirection()
    {
        (string SortBy, bool SortDesc)? captured = null;

        var cut = Render<SortableTableHeader>(ps => ps
            .Add(p => p.ColumnKey, "date")
            .Add(p => p.Label, "Order Date")
            .Add(p => p.CurrentSortBy, "date")
            .Add(p => p.SortDesc, true)
            .Add(p => p.OnSortChanged, EventCallback.Factory.Create<(string, bool)>(
                this, tuple => { captured = tuple; return Task.CompletedTask; })));

        await cut.Find("button").ClickAsync();

        captured.Should().NotBeNull();
        captured!.Value.SortBy.Should().Be("date");
        captured.Value.SortDesc.Should().BeFalse();
    }

    [Fact]
    public async Task Click_WhenInactive_DefaultsToDescending_ForDateColumn()
    {
        (string SortBy, bool SortDesc)? captured = null;

        var cut = Render<SortableTableHeader>(ps => ps
            .Add(p => p.ColumnKey, "date")
            .Add(p => p.Label, "Order Date")
            .Add(p => p.CurrentSortBy, "total")
            .Add(p => p.SortDesc, false)
            .Add(p => p.OnSortChanged, EventCallback.Factory.Create<(string, bool)>(
                this, tuple => { captured = tuple; return Task.CompletedTask; })));

        await cut.Find("button").ClickAsync();

        captured.Should().NotBeNull();
        captured!.Value.SortBy.Should().Be("date");
        captured.Value.SortDesc.Should().BeTrue();
    }

    [Fact]
    public async Task Click_WhenInactive_DefaultsToAscending_ForRestaurantColumn()
    {
        (string SortBy, bool SortDesc)? captured = null;

        var cut = Render<SortableTableHeader>(ps => ps
            .Add(p => p.ColumnKey, "restaurant")
            .Add(p => p.Label, "Restaurant")
            .Add(p => p.CurrentSortBy, "date")
            .Add(p => p.SortDesc, true)
            .Add(p => p.OnSortChanged, EventCallback.Factory.Create<(string, bool)>(
                this, tuple => { captured = tuple; return Task.CompletedTask; })));

        await cut.Find("button").ClickAsync();

        captured!.Value.SortBy.Should().Be("restaurant");
        captured.Value.SortDesc.Should().BeFalse();
    }
}
