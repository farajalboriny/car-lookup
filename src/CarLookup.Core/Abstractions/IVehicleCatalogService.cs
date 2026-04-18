using CarLookup.Core.Models;

namespace CarLookup.Core.Abstractions;

public interface IVehicleCatalogService
{
    Task<IReadOnlyList<VehicleMake>> GetAllMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleType>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleModel>> GetModelsAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken);
}
