using CarLookup.Core.Abstractions;
using CarLookup.Core.Configuration;
using CarLookup.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarLookup.Core.DependencyInjection;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCarLookupCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NhtsaOptions>(configuration.GetSection(NhtsaOptions.SectionName));
        services.AddMemoryCache();
        services.AddSingleton<IYearProvider, YearProvider>();
        services.AddScoped<IVehicleCatalogService, VehicleCatalogService>();
        return services;
    }
}
