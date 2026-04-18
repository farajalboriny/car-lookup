using System.Text.Json.Serialization;

namespace CarLookup.Core.Dtos;

public sealed class MakeDto
{
    [JsonPropertyName("Make_ID")]
    public int MakeId { get; set; }

    [JsonPropertyName("Make_Name")]
    public string MakeName { get; set; } = string.Empty;
}
