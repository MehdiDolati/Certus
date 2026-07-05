using Certus.Domain.Market.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class VolumeProfileTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var profile = new VolumeProfile(1000m, 600m, 400m);

        profile.TotalVolume.Should().Be(1000m);
        profile.BuyVolume.Should().Be(600m);
        profile.SellVolume.Should().Be(400m);
    }

    [Fact]
    public void BuyRatio_Should_Be_Calculated_Correctly()
    {
        var profile = new VolumeProfile(1000m, 600m, 400m);

        profile.BuyRatio.Should().Be(0.6m);
    }

    [Fact]
    public void SellRatio_Should_Be_Calculated_Correctly()
    {
        var profile = new VolumeProfile(1000m, 600m, 400m);

        profile.SellRatio.Should().Be(0.4m);
    }

    [Fact]
    public void BuyRatio_Should_Be_Zero_When_TotalVolume_Is_Zero()
    {
        var profile = new VolumeProfile(0m, 0m, 0m);

        profile.BuyRatio.Should().Be(0m);
    }

    [Fact]
    public void SellRatio_Should_Be_Zero_When_TotalVolume_Is_Zero()
    {
        var profile = new VolumeProfile(0m, 0m, 0m);

        profile.SellRatio.Should().Be(0m);
    }

    [Fact]
    public void IsBuyDominated_Should_Be_True_When_BuyVolume_Greater_Than_SellVolume()
    {
        var profile = new VolumeProfile(1000m, 600m, 400m);

        profile.IsBuyDominated.Should().BeTrue();
    }

    [Fact]
    public void IsBuyDominated_Should_Be_False_When_SellVolume_Greater_Than_BuyVolume()
    {
        var profile = new VolumeProfile(1000m, 400m, 600m);

        profile.IsBuyDominated.Should().BeFalse();
    }

    [Fact]
    public void IsBuyDominated_Should_Be_False_When_Volumes_Are_Equal()
    {
        var profile = new VolumeProfile(1000m, 500m, 500m);

        profile.IsBuyDominated.Should().BeFalse();
    }

    [Fact]
    public void Record_Equality_Should_Work()
    {
        var p1 = new VolumeProfile(1000m, 600m, 400m);
        var p2 = new VolumeProfile(1000m, 600m, 400m);

        p1.Should().Be(p2);
        p1.GetHashCode().Should().Be(p2.GetHashCode());
    }
}
