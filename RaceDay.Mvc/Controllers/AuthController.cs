using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class AuthController(RaceDayApiClient api) : Controller
{
    [HttpGet] public IActionResult Login() => View(new LoginVm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var (data, error, _) = await api.SendAsync<LoginReply>(HttpMethod.Post, "api/auth/login", vm, captureSession: true);
        if (data is null) { ModelState.AddModelError("", error ?? "Login failed."); return View(vm); }
        HttpContext.Session.SetInt32("UserId", data.UserId); HttpContext.Session.SetString("Name", data.FullName); HttpContext.Session.SetString("Role", data.Role);
        return data.Role == "Organiser" ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Index", "Events");
    }
    [HttpGet] public IActionResult Register() => View(new RegisterVm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Post, "api/auth/register", vm);
        if (error is not null) { ModelState.AddModelError("", error); return View(vm); }
        TempData["Notice"] = "Account created. Please sign in."; return RedirectToAction(nameof(Login));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await api.SendAsync<object>(HttpMethod.Post, "api/auth/logout"); HttpContext.Session.Clear(); return RedirectToAction(nameof(Login));
    }
    private sealed class LoginReply { public int UserId { get; set; } public string FullName { get; set; } = ""; public string Role { get; set; } = ""; }
}
