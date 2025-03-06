using BlandAIBankHeist.Web.Controllers;
using BlandAIBankHeist.Web.Models;
using BlandAIBankHeist.Web.Options;
using BlandAIBankHeist.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BlandAIBankHeist.Web.UnitTests;

public sealed class CallControllerTests
{
    public CallControllerTests()
    {
        _sut = new(NullLogger<CallController>.Instance, _apiOptionsMonitor, _blandApiService);

        _apiOptionsMonitor.CurrentValue
            .Returns(_fakeBlandApiOptions);
    }

    [Fact]
    public async Task IndexPost_WithInvalidPhoneNumber_HasModelValidationError_AndDoesNothing()
    {
        // Arrange
        const string expectedErrorMessage = "error";
        _sut.ModelState.AddModelError(nameof(CreateCallDTO.PhoneNumberToCall), expectedErrorMessage);

        var dummyDto = new CreateCallDTO("invalid");

        // Act
        var result = await _sut.Index(dummyDto) as ViewResult;

        // Assert
        result.Should().NotBeNull();

        result.ViewData.Should()
            .NotContainKey("SuccessMessage");
    }

    [Fact]
    public async Task IndexPost_WithValidPhoneNumber_AddsSuccessMessageForUser()
    {
        // Arrange
        var dummyDto = new CreateCallDTO("valid");

        // Act
        var result = await _sut.Index(dummyDto) as ViewResult;

        // Assert
        result.Should().NotBeNull();

        result.ViewData.Should()
            .ContainKey("SuccessMessage");
    }

    [Fact]
    public async Task IndexPost_WithValidPhoneNumber_TriesToQueueCall_WithExpectedPhoneNumberAndPathwayId()
    {
        // Arrange
        const string expectedPhoneNumber = nameof(expectedPhoneNumber);
        var createCallDto = new CreateCallDTO(expectedPhoneNumber);

        // Act
        await _sut.Index(createCallDto);

        // Assert
        await _blandApiService
            .Received(1)
            .TryToQueueCallAsync(expectedPhoneNumber, _fakePathwayId);
    }


    [Fact]
    public async Task IndexPost_WhenTryToQueueCallAsyncThrows_InformsUserOfError()
    {
        // Arrange
        var dummyDto = new CreateCallDTO("valid");

        _blandApiService.TryToQueueCallAsync(Arg.Any<string>(), Arg.Any<string>())
            .Throws<InvalidOperationException>();

        // Act
        var result = await _sut.Index(dummyDto) as ViewResult;

        // Assert
        result.Should().NotBeNull();

        result.ViewData.Should()
            .ContainKey("ErrorMessage");
    }

    private readonly CallController _sut;

    private const string _fakePathwayId = nameof(_fakePathwayId);

    private readonly BlandApiOptions _fakeBlandApiOptions = new() { ApiUrl = "", ApiKey = "", BankHeistIntroductionPathwayId = _fakePathwayId,
        BankHeistJobPathwayId = "", RecallDelayInSeconds = 0 };
    private readonly IOptionsMonitor<BlandApiOptions> _apiOptionsMonitor = Substitute.For<IOptionsMonitor<BlandApiOptions>>();
    private readonly IBlandApiService _blandApiService = Substitute.For<IBlandApiService>();
}
