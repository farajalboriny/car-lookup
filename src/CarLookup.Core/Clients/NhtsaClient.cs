using System.Net.Http.Json;
using CarLookup.Core.Abstractions;
using CarLookup.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace CarLookup.Core.Clients;

public sealed class NhtsaClient : INhtsaClient
{
    private const string JsonFormatQuery = "?format=json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<NhtsaClient> _logger;

    public NhtsaClient(HttpClient httpClient, ILogger<NhtsaClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MakeDto>> GetAllMakesAsync(CancellationToken cancellationToken)
    {
        string path = $"getallmakes{JsonFormatQuery}";
        NhtsaEnvelope<MakeDto> envelope = await SendAsync<MakeDto>(path, cancellationToken);
        return envelope.Results;
    }

    public async Task<IReadOnlyList<VehicleTypeDto>> GetVehicleTypesForMakeAsync(int makeId, CancellationToken cancellationToken)
    {
        string path = $"GetVehicleTypesForMakeId/{makeId}{JsonFormatQuery}";
        NhtsaEnvelope<VehicleTypeDto> envelope = await SendAsync<VehicleTypeDto>(path, cancellationToken);
        return envelope.Results;
    }

    public async Task<IReadOnlyList<ModelDto>> GetModelsAsync(int makeId, int year, string vehicleType, CancellationToken cancellationToken)
    {
        string encodedType = Uri.EscapeDataString(vehicleType);
        string path = $"GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{year}/vehicleType/{encodedType}{JsonFormatQuery}";
        NhtsaEnvelope<ModelDto> envelope = await SendAsync<ModelDto>(path, cancellationToken);
        return envelope.Results;
    }

    private async Task<NhtsaEnvelope<T>> SendAsync<T>(string relativePath, CancellationToken cancellationToken)
    {
        _logger.LogDebug("NHTSA request: {Path}", relativePath);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(relativePath, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new NhtsaApiException($"NHTSA request '{relativePath}' timed out.", statusCode: null, ex);
        }
        catch (HttpRequestException ex)
        {
            throw new NhtsaApiException($"NHTSA request '{relativePath}' failed: {ex.Message}", statusCode: null, ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            int statusCode = (int)response.StatusCode;
            _logger.LogWarning("NHTSA responded with {Status} for {Path}", statusCode, relativePath);
            throw new NhtsaApiException($"NHTSA returned HTTP {statusCode} for '{relativePath}'.", statusCode);
        }

        try
        {
            NhtsaEnvelope<T>? envelope = await response.Content.ReadFromJsonAsync<NhtsaEnvelope<T>>(cancellationToken: cancellationToken);
            if (envelope is null)
            {
                throw new NhtsaApiException($"NHTSA returned an empty body for '{relativePath}'.");
            }
            return envelope;
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new NhtsaApiException($"NHTSA returned malformed JSON for '{relativePath}'.", statusCode: (int)response.StatusCode, ex);
        }
    }
}
