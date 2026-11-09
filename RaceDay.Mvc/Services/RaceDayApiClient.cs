using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public sealed class RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor contextAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { PropertyNameCaseInsensitive = true };
    private ISession Session => contextAccessor.HttpContext!.Session;

    public async Task<(T? Data, string? Error, HttpStatusCode Status)> SendAsync<T>(HttpMethod method, string path, object? body = null, bool captureSession = false)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);
        var cookie = Session.GetString("ApiCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
        using var response = await factory.CreateClient("RaceDayApi").SendAsync(request);
        if (captureSession && response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            var sessionCookie = values.FirstOrDefault(v => v.StartsWith("RaceDay.Session=", StringComparison.OrdinalIgnoreCase));
            if (sessionCookie is not null) Session.SetString("ApiCookie", sessionCookie.Split(';')[0]);
        }
        var text = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            if (typeof(T) == typeof(string)) return ((T)(object)text, null, response.StatusCode);
            if (string.IsNullOrWhiteSpace(text)) return (default, null, response.StatusCode);
            try { return (JsonSerializer.Deserialize<T>(text, JsonOptions), null, response.StatusCode); }
            catch (JsonException) { return (default, "The API returned an unreadable response.", response.StatusCode); }
        }
        string? message = null;
        try { using var doc = JsonDocument.Parse(text); message = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null; }
        catch (JsonException) { }
        return (default, message ?? $"The API request failed ({(int)response.StatusCode}).", response.StatusCode);
    }

    public async Task<(string? Error, HttpStatusCode Status)> UploadAsync(string path, IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        form.Add(content, "file", Path.GetFileName(file.FileName));
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = form };
        var cookie = Session.GetString("ApiCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
        using var response = await factory.CreateClient("RaceDayApi").SendAsync(request);
        if (response.IsSuccessStatusCode) return (null, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        try { using var doc = JsonDocument.Parse(text); return (doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null, response.StatusCode); }
        catch (JsonException) { return ($"The API upload failed ({(int)response.StatusCode}).", response.StatusCode); }
    }
}
