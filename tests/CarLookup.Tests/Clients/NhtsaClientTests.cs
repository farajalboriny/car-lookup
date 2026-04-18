using System.Net;
using System.Text;
using CarLookup.Core.Clients;
using CarLookup.Core.Dtos;
using CarLookup.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CarLookup.Tests.Clients;

public class NhtsaClientTests
{
    private static NhtsaClient CreateClient(FakeHttpMessageHandler handler)
    {
        HttpClient http = new(handler) { BaseAddress = new Uri("https://test.example/api/vehicles/") };
        return new NhtsaClient(http, NullLogger<NhtsaClient>.Instance);
    }

    [Fact]
    public async Task GetAllMakesAsync_CallsCorrectUrl_AndParsesEnvelope()
    {
        NhtsaEnvelope<MakeDto> payload = new()
        {
            Count = 2,
            Message = "ok",
            Results = new[]
            {
                new MakeDto { MakeId = 448, MakeName = "TOYOTA" },
                new MakeDto { MakeId = 474, MakeName = "HONDA" }
            }
        };
        FakeHttpMessageHandler handler = new(_ => FakeHttpMessageHandler.JsonOk(payload));
        NhtsaClient sut = CreateClient(handler);

        IReadOnlyList<MakeDto> result = await sut.GetAllMakesAsync(CancellationToken.None);

        handler.Requests.Should().ContainSingle()
            .Which.RequestUri!.ToString().Should().Be("https://test.example/api/vehicles/getallmakes?format=json");
        result.Should().HaveCount(2);
        result[0].MakeId.Should().Be(448);
        result[0].MakeName.Should().Be("TOYOTA");
    }

    [Fact]
    public async Task GetVehicleTypesForMakeAsync_CallsCorrectUrl_AndParsesEnvelope()
    {
        NhtsaEnvelope<VehicleTypeDto> payload = new()
        {
            Count = 1,
            Results = new[] { new VehicleTypeDto { VehicleTypeId = 2, VehicleTypeName = "Passenger Car" } }
        };
        FakeHttpMessageHandler handler = new(_ => FakeHttpMessageHandler.JsonOk(payload));
        NhtsaClient sut = CreateClient(handler);

        IReadOnlyList<VehicleTypeDto> result = await sut.GetVehicleTypesForMakeAsync(448, CancellationToken.None);

        handler.Requests.Should().ContainSingle()
            .Which.RequestUri!.ToString().Should().Be("https://test.example/api/vehicles/GetVehicleTypesForMakeId/448?format=json");
        result.Should().ContainSingle().Which.VehicleTypeName.Should().Be("Passenger Car");
    }

    [Fact]
    public async Task GetModelsAsync_CallsCorrectUrl_WithUrlEncodedSegments()
    {
        NhtsaEnvelope<ModelDto> payload = new()
        {
            Count = 1,
            Results = new[] { new ModelDto { ModelId = 2208, ModelName = "Camry" } }
        };
        FakeHttpMessageHandler handler = new(_ => FakeHttpMessageHandler.JsonOk(payload));
        NhtsaClient sut = CreateClient(handler);

        IReadOnlyList<ModelDto> result = await sut.GetModelsAsync(448, 2020, "Passenger Car", CancellationToken.None);

        handler.Requests.Should().ContainSingle()
            .Which.RequestUri!.AbsoluteUri.Should().Be(
                "https://test.example/api/vehicles/GetModelsForMakeIdYear/makeId/448/modelyear/2020/vehicleType/Passenger%20Car?format=json");
        result.Should().ContainSingle().Which.ModelName.Should().Be("Camry");
    }

    [Fact]
    public async Task GetAllMakesAsync_Throws_OnNonSuccessStatus()
    {
        FakeHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("oops") });
        NhtsaClient sut = CreateClient(handler);

        Func<Task> act = () => sut.GetAllMakesAsync(CancellationToken.None);

        NhtsaApiException ex = (await act.Should().ThrowAsync<NhtsaApiException>()).Which;
        ex.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetAllMakesAsync_Throws_OnMalformedJson()
    {
        FakeHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json-at-all", Encoding.UTF8, "application/json")
            });
        NhtsaClient sut = CreateClient(handler);

        Func<Task> act = () => sut.GetAllMakesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<NhtsaApiException>()
            .Where(e => e.InnerException is System.Text.Json.JsonException);
    }

    [Fact]
    public async Task GetAllMakesAsync_Throws_OnCancellation()
    {
        FakeHttpMessageHandler handler = new(_ =>
        {
            throw new TaskCanceledException("timeout", new TimeoutException());
        });
        NhtsaClient sut = CreateClient(handler);

        Func<Task> act = () => sut.GetAllMakesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<NhtsaApiException>()
            .Where(e => e.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase));
    }
}
