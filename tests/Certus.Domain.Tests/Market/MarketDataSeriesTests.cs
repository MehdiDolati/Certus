using Certus.Domain.Market.Aggregates;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class MarketDataSeriesTests
{
    private static readonly Symbol BtcUsd = new("BTC/USD", AssetClass.Crypto);

    [Fact]
    public void Constructor_Should_Set_Id_Symbol_TimeFrame_And_LastUpdated()
    {
        var id = Guid.NewGuid();
        var series = new MarketDataSeries(id, BtcUsd, TimeFrame.OneHour);

        series.Id.Should().Be(id);
        series.Symbol.Should().Be(BtcUsd);
        series.TimeFrame.Should().Be(TimeFrame.OneHour);
        series.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Initialize_Empty_PriceBars()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneDay);

        series.PriceBars.Should().BeEmpty();
    }

    [Fact]
    public void PriceBars_Should_Be_ReadOnly()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneDay);

        series.PriceBars.Should().BeAssignableTo<IReadOnlyList<PriceBar>>();
    }

    [Fact]
    public void AddPriceBar_Should_Add_To_Collection()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);
        var bar = new PriceBar(DateTime.UtcNow, 100m, 110m, 90m, 105m, 5000m);

        series.AddPriceBar(bar);

        series.PriceBars.Should().HaveCount(1);
        series.PriceBars[0].Should().Be(bar);
    }

    [Fact]
    public void AddPriceBar_Should_Update_LastUpdated()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);
        var before = series.LastUpdated;

        Thread.Sleep(10);
        series.AddPriceBar(new PriceBar(DateTime.UtcNow, 100m, 110m, 90m, 105m, 5000m));

        series.LastUpdated.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void AddPriceBar_Multiple_Should_Accumulate()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);
        var bar1 = new PriceBar(DateTime.UtcNow, 100m, 110m, 90m, 105m, 5000m);
        var bar2 = new PriceBar(DateTime.UtcNow, 105m, 115m, 95m, 110m, 6000m);

        series.AddPriceBar(bar1);
        series.AddPriceBar(bar2);

        series.PriceBars.Should().HaveCount(2);
    }

    [Fact]
    public void UpdatePriceBars_Should_Replace_All_Bars()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);
        series.AddPriceBar(new PriceBar(DateTime.UtcNow, 100m, 110m, 90m, 105m, 5000m));
        series.AddPriceBar(new PriceBar(DateTime.UtcNow, 105m, 115m, 95m, 110m, 6000m));

        var newBars = new[]
        {
            new PriceBar(DateTime.UtcNow, 200m, 210m, 190m, 205m, 8000m)
        };
        series.UpdatePriceBars(newBars);

        series.PriceBars.Should().HaveCount(1);
        series.PriceBars[0].Open.Should().Be(200m);
    }

    [Fact]
    public void UpdatePriceBars_Should_With_Empty_Collection()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);
        series.AddPriceBar(new PriceBar(DateTime.UtcNow, 100m, 110m, 90m, 105m, 5000m));

        series.UpdatePriceBars([]);

        series.PriceBars.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePriceBars_Should_Update_LastUpdated()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);
        series.AddPriceBar(new PriceBar(DateTime.UtcNow, 100m, 110m, 90m, 105m, 5000m));
        var before = series.LastUpdated;

        Thread.Sleep(10);
        series.UpdatePriceBars([new PriceBar(DateTime.UtcNow, 200m, 210m, 190m, 205m, 8000m)]);

        series.LastUpdated.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Inherits_AggregateRoot_DomainEvents()
    {
        var series = new MarketDataSeries(Guid.NewGuid(), BtcUsd, TimeFrame.OneHour);

        series.DomainEvents.Should().NotBeNull();
        series.DomainEvents.Should().BeEmpty();
    }
}
