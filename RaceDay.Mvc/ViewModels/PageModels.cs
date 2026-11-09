using System.ComponentModel.DataAnnotations;
using RaceDay.Contracts;

namespace RaceDay.Mvc.ViewModels;

public sealed class LoginVm { [Required, EmailAddress] public string Email { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; }
public sealed class RegisterVm { [Required, MaxLength(45)] public string FirstName { get; set; } = ""; [Required, MaxLength(45)] public string LastName { get; set; } = ""; [Required, EmailAddress] public string Email { get; set; } = ""; [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = ""; [Required] public string Role { get; set; } = "Participant"; }
public sealed class EventPageVm { public EventDto Event { get; set; } = new(); public IReadOnlyList<CategoryDto> Categories { get; set; } = []; }
public sealed class EventSearchVm { public string? Query { get; set; } public string? EventType { get; set; } public bool UpcomingOnly { get; set; } public IReadOnlyList<EventDto> Events { get; set; } = []; }
public sealed class EventFormVm { public int EventId { get; set; } public IReadOnlyList<EventDto> Events { get; set; } = []; [Required, MaxLength(150)] public string Name { get; set; } = ""; [MaxLength(500)] public string? Description { get; set; } [Required] public DateTime Date { get; set; } = DateTime.Today.AddDays(14); [Required, MaxLength(150)] public string Location { get; set; } = ""; [Range(.01, 999.99)] public decimal DistanceKm { get; set; } = 5; [Required] public string EventType { get; set; } = "Run"; }
public sealed class CategoryFormVm { public int EventId { get; set; } [Required] public string Name { get; set; } = ""; [Range(.01, 999.99)] public decimal? DistanceKm { get; set; } [Range(1, 100000)] public int MaxParticipants { get; set; } = 100; [Range(0, 999999.99)] public decimal EntryFee { get; set; } }
public sealed class EnrolmentPageVm { public int EventId { get; set; } public string EventName { get; set; } = ""; public IReadOnlyList<EnrolmentDto> Enrolments { get; set; } = []; }
public sealed class ResultFormVm { public int EventId { get; set; } public int EnrolmentId { get; set; } [Required] public string FinishTime { get; set; } = "00:30:00"; [Range(1, int.MaxValue)] public int FinishPosition { get; set; } = 1; }
public sealed class ProfileVm { [Required, MaxLength(45)] public string FirstName { get; set; } = ""; [Required, MaxLength(45)] public string LastName { get; set; } = ""; public string Email { get; set; } = ""; public string Role { get; set; } = ""; public string? ProfileImageUrl { get; set; } }
