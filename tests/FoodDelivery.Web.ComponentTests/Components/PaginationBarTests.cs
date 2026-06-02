using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace FoodDelivery.Web.ComponentTests.Components;

public class PaginationBarTests : ComponentTestBase
{
    [Fact]
    public void Renders_RangeLabel_ForCurrentPage()
    {
        var cut = Render<PaginationBar>(ps => ps
            .Add(p => p.CurrentPage, 2)
            .Add(p => p.TotalPages, 5)
            .Add(p => p.TotalCount, 42)
            .Add(p => p.PageSize, 10)
            .Add(p => p.OnPageChanged, EventCallback.Factory.Create<int>(this, _ => Task.CompletedTask))
            .Add(p => p.OnPageSizeChanged, EventCallback.Factory.Create<int>(this, _ => Task.CompletedTask)));

        cut.Find(".pagination-bar-range").TextContent.Should().Contain("11–20 of 42");
    }

    [Fact]
    public async Task ClickNextPage_InvokesOnPageChanged()
    {
        int? newPage = null;

        var cut = Render<PaginationBar>(ps => ps
            .Add(p => p.CurrentPage, 2)
            .Add(p => p.TotalPages, 5)
            .Add(p => p.TotalCount, 50)
            .Add(p => p.PageSize, 10)
            .Add(p => p.OnPageChanged, EventCallback.Factory.Create<int>(this, page =>
            {
                newPage = page;
                return Task.CompletedTask;
            }))
            .Add(p => p.OnPageSizeChanged, EventCallback.Factory.Create<int>(this, _ => Task.CompletedTask)));

        await cut.Find("[aria-label='Next page']").ClickAsync();

        newPage.Should().Be(3);
    }

    [Fact]
    public async Task ChangePageSize_InvokesOnPageSizeChanged()
    {
        int? newSize = null;

        var cut = Render<PaginationBar>(ps => ps
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 3)
            .Add(p => p.TotalCount, 30)
            .Add(p => p.PageSize, 10)
            .Add(p => p.OnPageChanged, EventCallback.Factory.Create<int>(this, _ => Task.CompletedTask))
            .Add(p => p.OnPageSizeChanged, EventCallback.Factory.Create<int>(this, size =>
            {
                newSize = size;
                return Task.CompletedTask;
            })));

        cut.Find("select").Change(25);

        newSize.Should().Be(25);
    }

    [Fact]
    public void DoesNotRender_WhenNoPagesAndNoItems()
    {
        var cut = Render<PaginationBar>(ps => ps
            .Add(p => p.CurrentPage, 1)
            .Add(p => p.TotalPages, 0)
            .Add(p => p.TotalCount, 0)
            .Add(p => p.PageSize, 10)
            .Add(p => p.OnPageChanged, EventCallback.Factory.Create<int>(this, _ => Task.CompletedTask))
            .Add(p => p.OnPageSizeChanged, EventCallback.Factory.Create<int>(this, _ => Task.CompletedTask)));

        cut.Markup.Should().BeEmpty();
    }
}
