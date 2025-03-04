using System.Diagnostics;
using BlandAIBankHeist.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlandAIBankHeist.Web.Controllers;

public sealed class CallController : Controller
{
    public CallController(ILogger<CallController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Index([Bind("PhoneNumberToCall")]CreateCallDTO createCallDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("User tried to create call with invalid phone number.");
            return View(createCallDto);
        }

        _logger.LogInformation("User created a call with a valid phone number.");
        ViewData.Add("SuccessMessage", "Your call has been added to the queue! Please wait for the call!");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private readonly ILogger<CallController> _logger;
}
