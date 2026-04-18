using CarLookup.Core.Dtos;

namespace CarLookup.Core.Abstractions;

public interface INhtsaClient
{
    Task<IReadOnlyList<MakeDto>> GetAllMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesForMakeAsync(int makeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ModelDto>> GetModelsAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken);
}
