using System.Text.Json.Serialization;

namespace CarLookup.Core.Dtos;

public sealed class ModelDto
{
    [JsonPropertyName("Model_ID")]
    public int ModelId { get; set; }

    [JsonPropertyName("Model_Name")]
    public string ModelName { get; set; } = string.Empty;
}
