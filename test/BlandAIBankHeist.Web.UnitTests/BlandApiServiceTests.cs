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
        await _sut.Invoking(x => x.TryToQueueCallAsync(_expectedPhoneNumber, _fakePathwayId))
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
        await _sut.Invoking(x => x.TryToQueueCallAsync(_expectedPhoneNumber, _fakePathwayId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to parse response from BlandAI API.");
    }

    [Fact]
    public async Task TryToQueueCallAsync_ReturnsExpectedCallId_WhenResponseSuccessful_AndRespondsWithValidBlandAPIResponse()
    {
        // Arrange
        const string expectedCallId = nameof(expectedCallId);

        _httpMessageHandler.When(_fakeCallUrl)
            .Respond("application/json", $"{{\"status\": \"success\", \"call_id\": \"{expectedCallId}\"}}");

        // Act
        var callId = await _sut.TryToQueueCallAsync(_expectedPhoneNumber, _fakePathwayId);

        // Assert
        callId.Should().Be(expectedCallId);
    }

    [Fact]
    public async Task TryToQueueCallAsync_UsesExpectedPhoneNumberAndPathwayId_WhenUsingBlandAPI()
    {
        // Arrange
        _httpMessageHandler.Expect(_fakeCallUrl)
            .WithJsonContent(new QueueCallModel { PhoneNumber = _expectedPhoneNumber, PathwayId = _fakePathwayId })
            .Respond("application/json", $"{{\"status\": \"success\", \"call_id\": \"\"}}");

        // Act
        await _sut.TryToQueueCallAsync(_expectedPhoneNumber, _fakePathwayId);

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
        await _sut.TryToQueueCallAsync(_expectedPhoneNumber, _fakePathwayId);

        // Assert
        _httpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetCallDetailsAsync_ReturnsNull_WhenResponseFromApi_HasNonSuccessStatusCode()
    {
        // Arrange
        _httpMessageHandler.When(_fakeCallDetailsUrl)
            .Respond(HttpStatusCode.NotFound);

        // Act
        var callDetails = await _sut.GetCallDetailsAsync(_fakeCallId);

        // Assert
        callDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetCallDetailsAsync_ReturnsNull_WhenResponseIsIllFormed()
    {
        // Arrange
        _httpMessageHandler.When(_fakeCallDetailsUrl)
            .Respond("application/json", "{}");

        // Act
        var callDetails = await _sut.GetCallDetailsAsync(_fakeCallId);

        // Assert
        callDetails.Should().BeNull();
    }

    [Fact]
    public async Task GetCallDetailsAsync_ReturnsExpectedCallDetails_WhenResponseSuccessful_AndRespondsWithValidBlandAPIResponse()
    {
        // Arrange
        _httpMessageHandler.When(_fakeCallDetailsUrl)
            .Respond("application/json", $"{{\"call_id\": \"{_fakeCallId}\", \"to\": \"{_expectedPhoneNumber}\"}}");

        // Act
        var callDetails = await _sut.GetCallDetailsAsync(_fakeCallId);

        // Assert
        callDetails.Should().NotBeNull();

        callDetails.CallId.Should()
            .Be(_fakeCallId);

        callDetails.ToPhoneNumber.Should()
            .Be(_expectedPhoneNumber);
    }

    [Fact]
    public async Task GetCallDetailsAsync_UsesExpectedCallId_WhenUsingBlandAPI()
    {
        // Arrange
        _httpMessageHandler.Expect(_fakeCallDetailsUrl)
            .Respond("application/json", $"{{\"call_id\": \"{_fakeCallId}\", \"to\": \"{_expectedPhoneNumber}\"}}");

        // Act
        await _sut.GetCallDetailsAsync(_fakeCallId);

        // Assert
        _httpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetCallDetailsAsync_UsesExpectedApiKey_WhenUsingBlandAPI()
    {
        // Arrange
        _httpMessageHandler.Expect(_fakeCallDetailsUrl)
            .WithHeaders("Authorization", _fakeBlandApiOptions.ApiKey)
            .Respond("application/json", $"{{\"call_id\": \"{_fakeCallId}\", \"to\": \"{_expectedPhoneNumber}\"}}");

        // Act
        await _sut.GetCallDetailsAsync(_fakeCallId);

        // Assert
        _httpMessageHandler.VerifyNoOutstandingExpectation();
    }

    private readonly BlandApiService _sut;

    private readonly IOptionsMonitor<BlandApiOptions> _apiOptionsMonitor = Substitute.For<IOptionsMonitor<BlandApiOptions>>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly MockHttpMessageHandler _httpMessageHandler = new();

    private const string _fakeApiUrl = "https://www.FakeApiUrl.com";
    private const string _fakePathwayId = nameof(_fakePathwayId);
    private const string _fakeCallId = nameof(_fakeCallId);
    private const string _expectedPhoneNumber = nameof(_expectedPhoneNumber);

    private readonly BlandApiOptions _fakeBlandApiOptions = new() { ApiUrl = _fakeApiUrl, ApiKey = "FakeApiKey", BankHeistIntroductionPathwayId = _fakePathwayId };
    private readonly string _fakeCallUrl = $"{_fakeApiUrl}/v1/calls";
    private readonly string _fakeCallDetailsUrl = $"{_fakeApiUrl}/v1/calls/{_fakeCallId}";
}
