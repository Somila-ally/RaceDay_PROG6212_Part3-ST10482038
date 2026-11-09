using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers;

public class HomeController(IHttpContextAccessor accessor) : Controller
{
    public IActionResult Index()
    {
        var session = accessor.HttpContext!.Session;
        if (session.GetString("Role") == "Organiser") return RedirectToAction("Dashboard", "Organiser");
        if (session.GetString("Role") == "Participant") return RedirectToAction("Index", "Events");
        return RedirectToAction("Login", "Auth");
    }

    [Route("Home/Error")]
    public IActionResult Error() => View("~/Views/Shared/Error.cshtml", new RaceDay.Mvc.Models.ErrorViewModel
    {
        RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
