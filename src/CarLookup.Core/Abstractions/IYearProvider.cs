namespace CarLookup.Core.Abstractions;

public interface IYearProvider
{
    int EarliestYear { get; }

    int LatestYear { get; }

    IReadOnlyList<int> GetAllowedYears();
}
