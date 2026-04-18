using CarLookup.Core.Abstractions;
using CarLookup.Core.Configuration;
using CarLookup.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarLookup.Core.Services;

public sealed class VehicleCatalogService : IVehicleCatalogService
{
    private const string AllMakesCacheKey = "makes:all";

    private readonly INhtsaClient _nhtsaClient;
    private readonly IMemoryCache _cache;
    private readonly NhtsaOptions _options;
    private readonly ILogger<VehicleCatalogService> _logger;

    public VehicleCatalogService(
        INhtsaClient nhtsaClient,
        IMemoryCache cache,
        IOptions<NhtsaOptions> options,
        ILogger<VehicleCatalogService> logger)
    {
        _nhtsaClient = nhtsaClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleMake>> GetAllMakesAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(AllMakesCacheKey, out IReadOnlyList<VehicleMake>? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit: {Key}", AllMakesCacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss: {Key}", AllMakesCacheKey);
        var dtos = await _nhtsaClient.GetAllMakesAsync(cancellationToken);
        IReadOnlyList<VehicleMake> makes = dtos
            .Select(d => new VehicleMake(d.MakeId, d.MakeName))
            .ToList();

        _cache.Set(AllMakesCacheKey, makes, TimeSpan.FromMinutes(_options.MakesCacheMinutes));
        return makes;
    }

    public async Task<IReadOnlyList<VehicleType>> GetVehicleTypesAsync(int makeId, CancellationToken cancellationToken)
    {
        string cacheKey = $"types:{makeId}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<VehicleType>? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit: {Key}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss: {Key}", cacheKey);
        var dtos = await _nhtsaClient.GetVehicleTypesForMakeAsync(makeId, cancellationToken);
        IReadOnlyList<VehicleType> types = dtos
            .Select(d => new VehicleType(d.VehicleTypeId, d.VehicleTypeName))
            .ToList();

        _cache.Set(cacheKey, types, TimeSpan.FromMinutes(_options.VehicleTypesCacheMinutes));
        return types;
    }

    public async Task<IReadOnlyList<VehicleModel>> GetModelsAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken)
    {
        var dtos = await _nhtsaClient.GetModelsAsync(makeId, year, vehicleType, cancellationToken);
        return dtos
            .Select(d => new VehicleModel(d.ModelId, d.ModelName))
            .ToList();
    }
}
