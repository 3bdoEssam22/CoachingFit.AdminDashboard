using System.Net.Http.Json;
using System.Text.Json;
using CoachingFit.Identity.Shared.DTOs.Requests;
using CoachingFit.Identity.Shared.DTOs.Responses;
using CoachingFit.Identity.Shared.Wrappers;

namespace CoachingFit.AdminDashboard.Services;

public class AuthApiClient(HttpClient _http)
{
    public async Task<GenericResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/Auth/login", request, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new GenericResponse<AuthResponse>
            {
                StatusCode = (int)resp.StatusCode,
                Message = $"Empty response from server (HTTP {(int)resp.StatusCode} {resp.StatusCode}).",
            };
        }

        try
        {
            return JsonSerializer.Deserialize<GenericResponse<AuthResponse>>(raw,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new GenericResponse<AuthResponse>
                   {
                       StatusCode = (int)resp.StatusCode,
                       Message = "Null response payload.",
                   };
        }
        catch (JsonException)
        {
            var preview = raw.Length > 200 ? raw[..200] + "…" : raw;
            return new GenericResponse<AuthResponse>
            {
                StatusCode = (int)resp.StatusCode,
                Message = $"Server returned non-JSON (HTTP {(int)resp.StatusCode}): {preview}",
            };
        }
    }
}
