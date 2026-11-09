using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController(RaceDayApiClient api) : Controller
{
    private bool Allowed => HttpContext.Session.GetString("Role") == "Organiser";
    private IActionResult? CheckRole() => HttpContext.Session.GetInt32("UserId") is null ? RedirectToAction("Login", "Auth") : !Allowed ? Forbid() : null;
    public async Task<IActionResult> Dashboard()
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (events, error, _) = await api.SendAsync<List<EventDto>>(HttpMethod.Get, "api/events/mine");
        if (error is not null) TempData["Error"] = error;
        return View(events ?? []);
    }
    public async Task<IActionResult> Events(int? editId)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (events, error, _) = await api.SendAsync<List<EventDto>>(HttpMethod.Get, "api/events/mine");
        if (error is not null) TempData["Error"] = error;
        var vm = new EventFormVm { Events = events ?? [] };
        if (editId is int id)
        {
            var (ev, eventError, _) = await api.SendAsync<EventDto>(HttpMethod.Get, $"api/events/{id}");
            if (ev is null) TempData["Error"] = eventError;
            else { vm.EventId = ev.EventId; vm.Name = ev.Name; vm.Description = ev.Description; vm.Date = RaceDayClock.ToSouthAfricanTime(ev.Date); vm.Location = ev.Location; vm.DistanceKm = ev.DistanceKm; vm.EventType = ev.EventType; }
        }
        return View(vm);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEvent(EventFormVm vm)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        if (!ModelState.IsValid)
        {
            var (events, _, _) = await api.SendAsync<List<EventDto>>(HttpMethod.Get, "api/events/mine");
            vm.Events = events ?? [];
            return View("Events", vm);
        }
        var verb = vm.EventId == 0 ? HttpMethod.Post : HttpMethod.Put;
        var path = vm.EventId == 0 ? "api/events" : $"api/events/{vm.EventId}";
        var (_, error, _) = await api.SendAsync<object>(verb, path, new { name = vm.Name, vm.Description, date = RaceDayClock.ToUtc(vm.Date), vm.Location, vm.DistanceKm, vm.EventType });
        TempData[error is null ? "Notice" : "Error"] = error ?? "Event saved.";
        return RedirectToAction(nameof(Events));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Delete, $"api/events/{id}");
        TempData[error is null ? "Notice" : "Error"] = error ?? "Event deleted."; return RedirectToAction(nameof(Events));
    }
    public async Task<IActionResult> Categories(int id)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (ev, error, _) = await api.SendAsync<EventDto>(HttpMethod.Get, $"api/events/{id}");
        if (ev is null) { TempData["Error"] = error; return RedirectToAction(nameof(Events)); }
        var (cats, _, _) = await api.SendAsync<List<CategoryDto>>(HttpMethod.Get, $"api/events/{id}/categories");
        ViewBag.Event = ev; return View((id, (IReadOnlyList<CategoryDto>)(cats ?? [])));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryFormVm vm)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Post, $"api/events/{vm.EventId}/categories", new { name = vm.Name, vm.DistanceKm, maxParticipants = vm.MaxParticipants, entryFee = vm.EntryFee });
        TempData[error is null ? "Notice" : "Error"] = error ?? "Category added."; return RedirectToAction(nameof(Categories), new { id = vm.EventId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(CategoryDto vm)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Put, $"api/categories/{vm.CategoryId}", new { name = vm.Name, distanceKm = vm.DistanceKm, maxParticipants = vm.MaxParticipants, entryFee = vm.EntryFee });
        TempData[error is null ? "Notice" : "Error"] = error ?? "Category updated."; return RedirectToAction(nameof(Categories), new { id = vm.EventId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Delete, $"api/categories/{id}");
        TempData[error is null ? "Notice" : "Error"] = error ?? "Category deleted."; return RedirectToAction(nameof(Categories), new { id = eventId });
    }
    public async Task<IActionResult> Enrolments(int id)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (enrolments, error, _) = await api.SendAsync<List<EnrolmentDto>>(HttpMethod.Get, $"api/enrolments/event/{id}");
        var (ev, _, _) = await api.SendAsync<EventDto>(HttpMethod.Get, $"api/events/{id}");
        if (error is not null) TempData["Error"] = error;
        return View(new EnrolmentPageVm { EventId = id, EventName = ev?.Name ?? "Event", Enrolments = enrolments ?? [] });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(ResultFormVm vm)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Post, "api/results", new { vm.EnrolmentId, vm.FinishTime, vm.FinishPosition });
        TempData[error is null ? "Notice" : "Error"] = error ?? "Result recorded."; return RedirectToAction(nameof(Enrolments), new { id = vm.EventId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnrolmentStatus(int id, int eventId, string status)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Put, $"api/enrolments/{id}/status", new { status });
        TempData[error is null ? "Notice" : "Error"] = error ?? "Entry status updated."; return RedirectToAction(nameof(Enrolments), new { id = eventId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var guard = CheckRole(); if (guard is not null) return guard;
        var (error, _) = await api.UploadAsync($"api/images/events/{id}/banner", file);
        TempData[error is null ? "Notice" : "Error"] = error ?? "Event banner uploaded.";
        return RedirectToAction(nameof(Events));
    }
}
