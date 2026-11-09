using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController(RaceDayApiClient api) : Controller
{
    private IActionResult? CheckRole() => HttpContext.Session.GetInt32("UserId") is null ? RedirectToAction("Login", "Auth") : HttpContext.Session.GetString("Role") != "Participant" ? Forbid() : null;
    public async Task<IActionResult> MyEvents()
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (rows, error, _) = await api.SendAsync<List<EnrolmentDto>>(HttpMethod.Get, "api/enrolments/mine");
        if (error is not null) TempData["Error"] = error; return View(rows ?? []);
    }
    public async Task<IActionResult> MyResults()
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (rows, error, _) = await api.SendAsync<List<ResultDto>>(HttpMethod.Get, "api/results/mine");
        if (error is not null) TempData["Error"] = error; return View(rows ?? []);
    }
    public async Task<IActionResult> Profile()
    {
        if (HttpContext.Session.GetInt32("UserId") is null) return RedirectToAction("Login", "Auth");
        var (profile, error, _) = await api.SendAsync<ProfileDto>(HttpMethod.Get, "api/profile/me");
        if (profile is null) { TempData["Error"] = error; return RedirectToAction("Index", "Events"); }
        return View(new ProfileVm { FirstName = profile.FirstName, LastName = profile.LastName, Email = profile.Email, Role = profile.Role, ProfileImageUrl = profile.ProfileImageUrl });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVm vm, IFormFile? image)
    {
        if (HttpContext.Session.GetInt32("UserId") is null) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(vm);
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Put, "api/profile/me", new { vm.FirstName, vm.LastName });
        if (error is null && image is { Length: > 0 }) (error, _) = await api.UploadAsync("api/images/profile", image);
        TempData[error is null ? "Notice" : "Error"] = error ?? "Profile updated.";
        if (error is null) HttpContext.Session.SetString("Name", vm.FirstName + " " + vm.LastName);
        return RedirectToAction(nameof(Profile));
    }
}
