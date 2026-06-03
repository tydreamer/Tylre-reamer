using FoodDelivery.Web.Components.Browse;
using FluentAssertions;

namespace FoodDelivery.Web.UnitTests.Components;

public class PaginationPageEntriesTests
{
    [Theory]
    [InlineData(1, 1, new[] { 1 })]
    [InlineData(1, 2, new[] { 1, 2 })]
    [InlineData(2, 3, new[] { 1, 2, 3 })]
    [InlineData(1, 10, new[] { 1, 2, 3, -1, 10 })]
    [InlineData(3, 10, new[] { 1, 2, 3, -1, 10 })]
    [InlineData(5, 10, new[] { 1, -1, 5, -1, 10 })]
    [InlineData(7, 10, new[] { 1, -1, 7, -1, 10 })]
    [InlineData(8, 10, new[] { 1, -1, 8, 9, 10 })]
    [InlineData(10, 10, new[] { 1, -1, 8, 9, 10 })]
    public void Build_ReturnsExpectedSequence(int currentPage, int totalPages, int[] expected)
    {
        var entries = PaginationPageEntries.Build(currentPage, totalPages);

        entries.Should().HaveCount(expected.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            if (expected[i] == -1)
                entries[i].Should().BeNull();
            else
                entries[i].Should().Be(expected[i]);
        }
    }
}
