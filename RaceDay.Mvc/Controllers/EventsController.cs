using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class EventsController(RaceDayApiClient api) : Controller
{
    private bool SignedIn => HttpContext.Session.GetInt32("UserId") is not null;
    public async Task<IActionResult> Index(string? query, string? eventType, bool upcomingOnly = true)
    {
        if (!SignedIn) return RedirectToAction("Login", "Auth");
        var (events, error, _) = await api.SendAsync<List<EventDto>>(HttpMethod.Get, "api/events");
        if (error is not null) TempData["Error"] = error;
        var filtered = events ?? [];
        if (!string.IsNullOrWhiteSpace(query))
            filtered = filtered.Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || e.Location.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(eventType) && eventType != "All")
            filtered = filtered.Where(e => string.Equals(e.EventType, eventType, StringComparison.OrdinalIgnoreCase)).ToList();
        if (upcomingOnly) filtered = filtered.Where(e => e.Date >= DateTime.UtcNow).ToList();
        return View(new EventSearchVm { Query = query, EventType = eventType, UpcomingOnly = upcomingOnly, Events = filtered });
    }
    public async Task<IActionResult> Details(int id)
    {
        if (!SignedIn) return RedirectToAction("Login", "Auth");
        var (ev, error, _) = await api.SendAsync<EventDto>(HttpMethod.Get, $"api/events/{id}");
        if (ev is null) { TempData["Error"] = error; return RedirectToAction(nameof(Index)); }
        var (cats, _, _) = await api.SendAsync<List<CategoryDto>>(HttpMethod.Get, $"api/events/{id}/categories");
        return View(new EventPageVm { Event = ev, Categories = cats ?? [] });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (!SignedIn) return RedirectToAction("Login", "Auth");
        if (HttpContext.Session.GetString("Role") != "Participant") return Forbid();
        var (_, error, _) = await api.SendAsync<object>(HttpMethod.Post, "api/enrolments", new { eventId, categoryId });
        TempData[error is null ? "Notice" : "Error"] = error ?? "You're entered. Good luck!";
        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}
