using System.Text.Json.Serialization;

namespace CarLookup.Core.Dtos;

public sealed class NhtsaEnvelope<T>
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }

    [JsonPropertyName("SearchCriteria")]
    public string? SearchCriteria { get; set; }

    [JsonPropertyName("Results")]
    public IReadOnlyList<T> Results { get; set; } = Array.Empty<T>();
}
