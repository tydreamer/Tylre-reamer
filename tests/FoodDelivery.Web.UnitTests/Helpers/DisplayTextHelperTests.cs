using FoodDelivery.Web.Helpers;

namespace FoodDelivery.Web.UnitTests.Helpers;

public class DisplayTextHelperTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("SAVE10", "SAVE10")]
    [InlineData("ABCDEFGHIJKLMNOP", "ABCDEFGHIJKLMNOP")]
    public void TruncateWithEllipsis_leaves_short_values_unchanged(string? input, string expected) =>
        Assert.Equal(expected, DisplayTextHelper.TruncateWithEllipsis(input));

    [Fact]
    public void TruncateWithEllipsis_appends_ellipsis_when_over_limit()
    {
        var input = "ABCDEFGHIJKLMNOPQR";
        var result = DisplayTextHelper.TruncateWithEllipsis(input, DisplayTextHelper.CouponCodeDisplayLength);

        Assert.Equal("ABCDEFGHIJKLM...", result);
        Assert.True(result.Length <= DisplayTextHelper.CouponCodeDisplayLength);
    }
}
