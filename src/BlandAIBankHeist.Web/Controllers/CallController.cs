using System.Diagnostics;
using BlandAIBankHeist.Web.Models;
using BlandAIBankHeist.Web.Options;
using BlandAIBankHeist.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlandAIBankHeist.Web.Controllers;

public sealed class CallController : Controller
{
    public CallController(ILogger<CallController> logger, IOptionsMonitor<BlandApiOptions> apiOptions, IBlandApiService blandApiService)
    {
        _logger = logger;
        _apiOptions = apiOptions;
        _blandApiService = blandApiService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index([Bind("PhoneNumberToCall")]CreateCallDTO createCallDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("User tried to create call with invalid phone number.");
            return View(createCallDto);
        }

        try
        {
            var callId = await _blandApiService.TryToQueueCallAsync(createCallDto.PhoneNumberToCall, _apiOptions.CurrentValue.BankHeistIntroductionPathwayId);
            _logger.LogInformation("User created a call with a valid phone number with call ID {CallId}.", callId);
            ViewData.Add("SuccessMessage", "Your call has been added to the queue! Please wait for the call!");
            return View();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Failed to create call with exception message {Message}", ex.Message);

            // TODO: User friendly error messages
            ViewData.Add("ErrorMessage", "Failed to add call to the queue! Please try again soon.");
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private readonly ILogger<CallController> _logger;
    private readonly IOptionsMonitor<BlandApiOptions> _apiOptions;
    private readonly IBlandApiService _blandApiService;
}
