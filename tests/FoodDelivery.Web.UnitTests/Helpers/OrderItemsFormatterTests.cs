using FluentAssertions;
using FoodDelivery.Web.Helpers;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.UnitTests.Helpers;

public class OrderItemsFormatterTests
{
    [Fact]
    public void FormatForDisplay_EmptyItems_ReturnsDash()
    {
        var (display, full) = OrderItemsFormatter.FormatForDisplay([]);

        display.Should().Be("—");
        full.Should().BeEmpty();
    }

    [Fact]
    public void FormatForDisplay_SingleItem_ShowsNameOnly()
    {
        var items = new[] { new OrderItemResponse(1, "Margherita", 1, 12m) };

        var (display, full) = OrderItemsFormatter.FormatForDisplay(items);

        display.Should().Be("Margherita");
        full.Should().Be("Margherita");
    }

    [Fact]
    public void FormatForDisplay_MultipleQuantities_IncludesMultiplier()
    {
        var items = new[]
        {
            new OrderItemResponse(1, "Burger", 2, 10m),
            new OrderItemResponse(2, "Fries", 1, 4m)
        };

        var full = OrderItemsFormatter.FormatFull(items);

        full.Should().Be("Burger × 2, Fries");
    }

    [Fact]
    public void FormatForDisplay_LongList_TruncatesWithEllipsis()
    {
        var longName = new string('A', 90);
        var items = new[] { new OrderItemResponse(1, longName, 1, 1m) };

        var (display, full) = OrderItemsFormatter.FormatForDisplay(items);

        full.Length.Should().BeGreaterThan(OrderItemsFormatter.MaxDisplayLength);
        display.Should().EndWith("...");
        display.Length.Should().Be(OrderItemsFormatter.MaxDisplayLength);
    }
}
