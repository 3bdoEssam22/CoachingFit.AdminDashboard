using System.Net.Http.Json;
using CoachingFit.Identity.Shared.DTOs.Requests;
using CoachingFit.Identity.Shared.DTOs.Responses;
using CoachingFit.Identity.Shared.Wrappers;

namespace CoachingFit.AdminDashboard.Services;

public class AuthApiClient(HttpClient _http)
{
    public async Task<GenericResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/Auth/login", request, ct);
        var body = await resp.Content.ReadFromJsonAsync<GenericResponse<AuthResponse>>(ct)
                   ?? new GenericResponse<AuthResponse>
                   {
                       StatusCode = (int)resp.StatusCode,
                       Message = "Empty response from server.",
                   };
        return body;
    }
}
