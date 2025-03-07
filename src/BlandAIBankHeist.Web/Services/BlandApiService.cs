using BlandAIBankHeist.Web.Models;
using BlandAIBankHeist.Web.Options;
using BlandAIBankHeist.Web.Responses;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BlandAIBankHeist.Web.Services;

public class BlandApiService : IBlandApiService
{
    public BlandApiService(ILogger<BlandApiService> logger, IOptionsMonitor<BlandApiOptions> apiOptions, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _apiOptions = apiOptions;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> TryToQueueCallAsync(string phoneNumber, string pathwayId, string voice, string? backgroundTrack = null)
    {
        var client = CreateAuthorizedHttpClientForBlandApi();

        var response = await client.PostAsJsonAsync("/v1/calls", new QueueCallModel { PhoneNumber = phoneNumber, PathwayId = pathwayId, Voice = voice,
            BackgroundTrack = backgroundTrack });
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to queue a call with the BlandAI API with the code {Code} and reponse {Response}.",
                response.StatusCode, await response.Content.ReadAsStringAsync());

            throw new InvalidOperationException("Failed to queue call with the BlandAI API.");
        }

        try
        {
            var successfulQueueCallResponse = await response.Content.ReadFromJsonAsync<SuccessfulQueueCallResponse>();

            _logger.LogInformation("Call was successfully queued with call id of {CallId} and pathway id {PathwayId}.",
                successfulQueueCallResponse!.CallId, pathwayId);
            return successfulQueueCallResponse.CallId;
        }
        catch (JsonException)
        {
            _logger.LogError("Successful status code from BlandAI API, however, an invalid response of {Response} was returned.",
                await response.Content.ReadAsStringAsync());

            throw new InvalidOperationException("Failed to parse response from BlandAI API.");
        }
    }

    public async Task<CallDetailsModel?> GetCallDetailsAsync(string callId)
    {
        var client = CreateAuthorizedHttpClientForBlandApi();

        var response = await client.GetAsync($"/v1/calls/{callId}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get call details from BlandAPI with call id {CallID}.", callId);
            return null;
        }

        try
        {
            var callDetails = await response.Content.ReadFromJsonAsync<CallDetailsModel>();

            _logger.LogInformation("Successfully got call details from BlandAPI with call id {CallID}.", callId);
            return callDetails;
        }
        catch (JsonException)
        {
            _logger.LogError("Successful status code from BlandAI API, however, an invalid response of {Response} was returned.",
                await response.Content.ReadAsStringAsync());

            return null;
        }
    }

    private HttpClient CreateAuthorizedHttpClientForBlandApi()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new(_apiOptions.CurrentValue.ApiUrl);
        client.DefaultRequestHeaders.Add("Authorization", _apiOptions.CurrentValue.ApiKey);
        return client;
    }

    private readonly ILogger<BlandApiService> _logger;
    private readonly IOptionsMonitor<BlandApiOptions> _apiOptions;
    private readonly IHttpClientFactory _httpClientFactory;
}
