using BlandAIBankHeist.Web.Controllers;
using BlandAIBankHeist.Web.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlandAIBankHeist.Web.UnitTests;

public sealed class CallControllerTests
{
    public CallControllerTests()
    {
        _sut = new(NullLogger<CallController>.Instance);
    }

    [Fact]
    public void IndexPost_WithInvalidPhoneNumber_HasModelValidationError_AndDoesNothing()
    {
        // Arrange
        const string expectedErrorMessage = "error";
        _sut.ModelState.AddModelError(nameof(CreateCallDTO.PhoneNumberToCall), expectedErrorMessage);

        var dummyDto = new CreateCallDTO("invalid");

        // Act
        var result = _sut.Index(dummyDto) as ViewResult;

        // Assert
        result.Should().NotBeNull();

        result.ViewData.Should()
            .NotContainKey("SuccessMessage");
    }

    [Fact]
    public void IndexPost_WithValidPhoneNumber_TriesToQueueCall()
    {
        // Arrange
        var dummyDto = new CreateCallDTO("valid");

        // Act
        var result = _sut.Index(dummyDto) as ViewResult;

        // Assert
        result.Should().NotBeNull();

        result.ViewData.Should()
            .ContainKey("SuccessMessage");
    }

    private readonly CallController _sut;
}
