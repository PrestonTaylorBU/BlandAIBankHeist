using BlandAIBankHeist.Web.Models;
using BlandAIBankHeist.Web.Options;
using BlandAIBankHeist.Web.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Net;

namespace BlandAIBankHeist.Web.UnitTests;

public sealed class BlandApiServiceTests
{
    public BlandApiServiceTests()
    {
        _sut = new(NullLogger<BlandApiService>.Instance, _apiOptionsMonitor, _httpClientFactory);

        _apiOptionsMonitor.CurrentValue
            .Returns(_fakeBlandApiOptions);

        _httpClientFactory.CreateClient()
            .Returns(new HttpClient(_httpMessageHandler));
    }

    [Fact]
    public async Task TryToQueueCallAsync_ThrowsInvalidOperationException_WhenResponseFromApi_HasNonSuccessStatusCode()
    {
        // Arrange
        _httpMessageHandler.When(_fakeCallUrl)
            .Respond(HttpStatusCode.NotFound);

        // Act
        // Assert
        await _sut.Invoking(x => x.TryToQueueCallAsync(_expectedPhoneNumber))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to queue call with the BlandAI API.");
    }

    [Fact]
    public async Task TryToQueueCallAsync_ThrowsInvalidOperationException_WhenResponseIsIllFormed()
    {
        // Arrange
        _httpMessageHandler.When(_fakeCallUrl)
            .Respond("application/json", "{}");

        // Act
        // Assert
        await _sut.Invoking(x => x.TryToQueueCallAsync(_expectedPhoneNumber))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to parse response from BlandAI API.");
    }

    [Fact]
    public async Task TryToQueueCallAsync_ReturnsCallId_WhenResponseSuccessful_AndRespondsWithValidBlandAPIResponse()
    {
        // Arrange
        const string expectedCallId = nameof(expectedCallId);

        _httpMessageHandler.When(_fakeCallUrl)
            .Respond("application/json", $"{{\"status\": \"success\", \"call_id\": \"{expectedCallId}\"}}");

        // Act
        var callId = await _sut.TryToQueueCallAsync(_expectedPhoneNumber);

        // Assert
        callId.Should().Be(expectedCallId);
    }

    [Fact]
    public async Task TryToQueueCallAsync_UsesExpectedPhoneNumber_WhenUsingBlandAPI()
    {
        // Arrange
        _httpMessageHandler.Expect(_fakeCallUrl)
            .WithJsonContent(new QueueCallModel { PhoneNumber = _expectedPhoneNumber, Task = "Greet User like a Bank Teller." })
            .Respond("application/json", $"{{\"status\": \"success\", \"call_id\": \"\"}}");

        // Act
        await _sut.TryToQueueCallAsync(_expectedPhoneNumber);

        // Assert
        _httpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TryToQueueCallAsync_UsesExpectedApiKey_WhenUsingBlandAPI()
    {
        // Arrange
        _httpMessageHandler.Expect(_fakeCallUrl)
            .WithHeaders("Authorization", _fakeBlandApiOptions.ApiKey)
            .Respond("application/json", $"{{\"status\": \"success\", \"call_id\": \"\"}}");

        // Act
        await _sut.TryToQueueCallAsync(_expectedPhoneNumber);

        // Assert
        _httpMessageHandler.VerifyNoOutstandingExpectation();
    }

    private readonly BlandApiService _sut;

    private readonly IOptionsMonitor<BlandApiOptions> _apiOptionsMonitor = Substitute.For<IOptionsMonitor<BlandApiOptions>>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly MockHttpMessageHandler _httpMessageHandler = new();

    private const string _fakeApiUrl = "https://www.FakeApiUrl.com";
    private const string _expectedPhoneNumber = nameof(_expectedPhoneNumber);

    private readonly BlandApiOptions _fakeBlandApiOptions = new() { ApiUrl = _fakeApiUrl, ApiKey = "FakeApiKey" };
    private readonly string _fakeCallUrl = $"{_fakeApiUrl}/v1/calls";
}
