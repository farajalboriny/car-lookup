using System.Text.Json.Serialization;

namespace CarLookup.Core.Dtos;

public sealed class VehicleTypeDto
{
    [JsonPropertyName("VehicleTypeId")]
    public int VehicleTypeId { get; set; }

    [JsonPropertyName("VehicleTypeName")]
    public string VehicleTypeName { get; set; } = string.Empty;
}
