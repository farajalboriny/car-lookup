using CarLookup.Core.Configuration;
using CarLookup.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CarLookup.Tests.Services;

public class YearProviderTests
{
    private static YearProvider CreateProvider(int earliestYear, int currentYear)
    {
        NhtsaOptions options = new() { EarliestYear = earliestYear };
        IOptions<NhtsaOptions> optionsWrapper = Options.Create(options);
        return new YearProvider(optionsWrapper, () => new DateTime(currentYear, 6, 1));
    }

    [Fact]
    public void EarliestAndLatestYear_ReflectConfigurationAndClock()
    {
        YearProvider sut = CreateProvider(earliestYear: 1995, currentYear: 2026);

        sut.EarliestYear.Should().Be(1995);
        sut.LatestYear.Should().Be(2026);
    }

    [Fact]
    public void GetAllowedYears_ReturnsDescendingRange()
    {
        YearProvider sut = CreateProvider(earliestYear: 1995, currentYear: 2026);

        IReadOnlyList<int> years = sut.GetAllowedYears();

        years.Should().HaveCount(32);
        years[0].Should().Be(2026);
        years[^1].Should().Be(1995);
    }
}
