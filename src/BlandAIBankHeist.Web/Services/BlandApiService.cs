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

    public async Task<string> TryToQueueCallAsync(string phoneNumber)
    {
        var client = CreateAuthorizedHttpClientForBlandApi();

        var response = await client.PostAsJsonAsync("/v1/calls", new QueueCallModel { PhoneNumber = phoneNumber, Task = "Greet User like a Bank Teller." });
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to queue a call with the BlandAI API with the code {Code} and reponse {Response}.",
                response.StatusCode, await response.Content.ReadAsStringAsync());

            throw new InvalidOperationException("Failed to queue call with the BlandAI API.");
        }

        try
        {
            var successfulQueueCallResponse = await response.Content.ReadFromJsonAsync<SuccessfulQueueCallResponse>();

            _logger.LogInformation("Call was successfully queued with call id of {CallId}.", successfulQueueCallResponse!.CallId);
            return successfulQueueCallResponse.CallId;
        }
        catch (JsonException)
        {
            _logger.LogError("Successful status code from BlandAI API, however, an invalid response of {Response} was returned.",
                await response.Content.ReadAsStringAsync());

            throw new InvalidOperationException("Failed to parse response from BlandAI API.");
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
