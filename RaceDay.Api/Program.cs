using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay system: Organiser/Participant accounts, events, categories, " +
                      "enrolments and results, with session-based authentication and role-based access.\n\n" +
                      "**How to try it:** call POST /api/auth/register, then POST /api/auth/login. " +
                      "Swagger UI keeps the session cookie, so every protected endpoint below then works " +
                      "for the role you logged in as (401 = not logged in, 403 = wrong role)."
    });

    options.EnableAnnotations();

    // Include the XML documentation of both the API and the shared Contracts project.
    foreach (var xmlFile in new[] { "RaceDay.Api.xml", "RaceDay.Contracts.xml" })
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }
});

// Database (SQL Server, EF Core Code First)
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session-based authentication (UserId and Role are stored in the server-side session after login)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = "RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Password hashing (salted PBKDF2 via ASP.NET Core's PasswordHasher)
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<BlobStorageService>();

var app = builder.Build();

// Swagger is always enabled so the marker can open it without extra configuration.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
});

// Local development uses the documented HTTP URL; production HTTPS should terminate at its proxy.
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the Program class to the test project (WebApplicationFactory<Program>).
public partial class Program { }
