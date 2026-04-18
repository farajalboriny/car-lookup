using CarLookup.Core.Abstractions;
using CarLookup.Core.Configuration;
using CarLookup.Core.Dtos;
using CarLookup.Core.Models;
using CarLookup.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CarLookup.Tests.Services;

public class VehicleCatalogServiceTests
{
    private readonly Mock<INhtsaClient> _nhtsaClient = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IOptions<NhtsaOptions> _options = Options.Create(new NhtsaOptions
    {
        MakesCacheMinutes = 60,
        VehicleTypesCacheMinutes = 30
    });

    private VehicleCatalogService CreateSut()
    {
        return new VehicleCatalogService(
            _nhtsaClient.Object,
            _cache,
            _options,
            NullLogger<VehicleCatalogService>.Instance);
    }

    [Fact]
    public async Task GetAllMakesAsync_MapsDtosToDomainModels()
    {
        _nhtsaClient
            .Setup(c => c.GetAllMakesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new MakeDto { MakeId = 448, MakeName = "TOYOTA" },
                new MakeDto { MakeId = 474, MakeName = "HONDA" }
            });
        VehicleCatalogService sut = CreateSut();

        IReadOnlyList<VehicleMake> result = await sut.GetAllMakesAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(448);
        result[0].Name.Should().Be("TOYOTA");
    }

    [Fact]
    public async Task GetAllMakesAsync_CachesResult_AcrossMultipleCalls()
    {
        _nhtsaClient
            .Setup(c => c.GetAllMakesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new MakeDto { MakeId = 1, MakeName = "ACME" } });
        VehicleCatalogService sut = CreateSut();

        _ = await sut.GetAllMakesAsync(CancellationToken.None);
        _ = await sut.GetAllMakesAsync(CancellationToken.None);

        _nhtsaClient.Verify(c => c.GetAllMakesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVehicleTypesAsync_CachesPerMakeId()
    {
        _nhtsaClient
            .Setup(c => c.GetVehicleTypesForMakeAsync(448, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new VehicleTypeDto { VehicleTypeId = 2, VehicleTypeName = "Passenger Car" } });
        _nhtsaClient
            .Setup(c => c.GetVehicleTypesForMakeAsync(474, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new VehicleTypeDto { VehicleTypeId = 7, VehicleTypeName = "Motorcycle" } });
        VehicleCatalogService sut = CreateSut();

        _ = await sut.GetVehicleTypesAsync(448, CancellationToken.None);
        _ = await sut.GetVehicleTypesAsync(448, CancellationToken.None);
        IReadOnlyList<VehicleType> second = await sut.GetVehicleTypesAsync(474, CancellationToken.None);

        _nhtsaClient.Verify(c => c.GetVehicleTypesForMakeAsync(448, It.IsAny<CancellationToken>()), Times.Once);
        _nhtsaClient.Verify(c => c.GetVehicleTypesForMakeAsync(474, It.IsAny<CancellationToken>()), Times.Once);
        second.Should().ContainSingle().Which.Name.Should().Be("Motorcycle");
    }

    [Fact]
    public async Task GetModelsAsync_DelegatesToClient_AndMapsResults()
    {
        _nhtsaClient
            .Setup(c => c.GetModelsAsync(448, 2020, "Passenger Car", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new ModelDto { ModelId = 2208, ModelName = "Camry" } });
        VehicleCatalogService sut = CreateSut();

        IReadOnlyList<VehicleModel> result = await sut.GetModelsAsync(448, 2020, "Passenger Car", CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(2208);
        result[0].Name.Should().Be("Camry");
    }
}
