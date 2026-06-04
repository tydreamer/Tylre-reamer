using Bunit;
using FluentAssertions;

namespace FoodDelivery.Web.ComponentTests.Components;

public class OrderDetailMetaTests : ComponentTestBase
{
    [Fact]
    public void Renders_RestaurantName()
    {
        var cut = Render<OrderDetailMeta>(ps => ps
            .Add(p => p.RestaurantName, "Test Bistro")
            .Add(p => p.CreatedAt, DateTime.UtcNow));

        cut.Find(".order-detail-meta__value--restaurant").TextContent.Should().Be("Test Bistro");
    }

    [Fact]
    public void Renders_CustomerName_WhenProvided()
    {
        var cut = Render<OrderDetailMeta>(ps => ps
            .Add(p => p.RestaurantName, "Bistro")
            .Add(p => p.CustomerName, "John Doe")
            .Add(p => p.CreatedAt, DateTime.UtcNow));

        cut.Find(".order-detail-meta__value--customer").TextContent.Should().Be("John Doe");
    }

    [Fact]
    public void DoesNotRender_CustomerSection_WhenCustomerNameIsNull()
    {
        var cut = Render<OrderDetailMeta>(ps => ps
            .Add(p => p.RestaurantName, "Bistro")
            .Add(p => p.CreatedAt, DateTime.UtcNow));

        cut.FindAll(".order-detail-meta__item--customer").Should().BeEmpty();
    }

    [Fact]
    public void Renders_CreatedAt_AsTimeElement()
    {
        var date = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc);

        var cut = Render<OrderDetailMeta>(ps => ps
            .Add(p => p.RestaurantName, "Bistro")
            .Add(p => p.CreatedAt, date));

        cut.Find("time").GetAttribute("datetime").Should().Contain("2026-03-15");
    }
}
