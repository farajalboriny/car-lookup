using CarLookup.Core.Abstractions;
using CarLookup.Core.Configuration;
using Microsoft.Extensions.Options;

namespace CarLookup.Core.Services;

public sealed class YearProvider : IYearProvider
{
    private readonly NhtsaOptions _options;
    private readonly Func<DateTime> _clock;

    public YearProvider(IOptions<NhtsaOptions> options)
        : this(options, () => DateTime.UtcNow)
    {
    }

    public YearProvider(IOptions<NhtsaOptions> options, Func<DateTime> clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public int EarliestYear => _options.EarliestYear;

    public int LatestYear => _clock().Year;

    public IReadOnlyList<int> GetAllowedYears()
    {
        int latest = LatestYear;
        int earliest = EarliestYear;
        List<int> years = new(latest - earliest + 1);
        for (int year = latest; year >= earliest; year--)
        {
            years.Add(year);
        }
        return years;
    }
}
