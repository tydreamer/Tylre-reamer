using FluentAssertions;

namespace FoodDelivery.API.UnitTests.Helpers;

public class MealImageUrlBuilderTests
{
    [Fact]
    public void Build_UsesFirstWordWithThreeOrMoreLetters()
    {
        var url = MealImageUrlBuilder.Build("Grilled Chicken Salad", lockSeed: 500);

        url.Should().StartWith("https://loremflickr.com/600/400/food,grilled?lock=");
        url.Should().EndWith("500");
    }

    [Fact]
    public void Build_FallsBackToFood_WhenNoSuitableWord()
    {
        var url = MealImageUrlBuilder.Build("A B", lockSeed: 100);

        url.Should().Contain("food,food?");
    }

    [Fact]
    public void Build_ClampsLockSeedToValidRange()
    {
        var url = MealImageUrlBuilder.Build("Pasta", lockSeed: -99999);

        url.Should().MatchRegex(@"lock=\d+");
        url.Should().NotContain("lock=-");
    }

    [Fact]
    public void SeedFrom_IsDeterministicForSameInputs()
    {
        var a = MealImageUrlBuilder.SeedFrom("Bistro", "Pasta");
        var b = MealImageUrlBuilder.SeedFrom("Bistro", "Pasta");
        var c = MealImageUrlBuilder.SeedFrom("Bistro", "Risotto");

        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}
