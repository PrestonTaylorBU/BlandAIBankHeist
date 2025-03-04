using System.Diagnostics;
using BlandAIBankHeist.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlandAIBankHeist.Web.Controllers;

public class CallController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
