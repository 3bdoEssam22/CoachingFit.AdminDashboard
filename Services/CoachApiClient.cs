using System.Net.Http.Json;
using System.Text;
using CoachingFit.Identity.Shared.DTOs.Responses;
using CoachingFit.User.Shared.DTOs.Responses;
using IdentityGenericResponse = CoachingFit.Identity.Shared.Wrappers.GenericResponse<bool>;
using IdentityIdsResponse = CoachingFit.Identity.Shared.Wrappers.GenericResponse<System.Collections.Generic.IEnumerable<string>>;
using IdentityStatsResponse = CoachingFit.Identity.Shared.Wrappers.GenericResponse<CoachingFit.Identity.Shared.DTOs.Responses.AdminStatsResponse>;
using UserProfilesResponse = CoachingFit.User.Shared.Wrappers.GenericResponse<System.Collections.Generic.IEnumerable<CoachingFit.User.Shared.DTOs.Responses.CoachProfileResponse>>;

namespace CoachingFit.AdminDashboard.Services;

public class CoachApiClient(HttpClient _http)
{
    public async Task<IEnumerable<string>> GetPendingUserIdsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<IdentityIdsResponse>("api/Auth/coaches/pending", ct);
        return resp?.Data ?? [];
    }

    public async Task<IEnumerable<string>> GetAllUserIdsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<IdentityIdsResponse>("api/Auth/coaches/all", ct);
        return resp?.Data ?? [];
    }

    public async Task<AdminStatsResponse?> GetStatsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<IdentityStatsResponse>("api/Auth/stats", ct);
        return resp?.Data;
    }

    public async Task<IEnumerable<CoachProfileResponse>> GetProfilesByUserIdsAsync(
        IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0)
            return [];

        var query = new StringBuilder("api/CoachProfile/pending?");
        for (int i = 0; i < ids.Count; i++)
        {
            if (i > 0) query.Append('&');
            query.Append("userIds=").Append(Uri.EscapeDataString(ids[i]));
        }

        var resp = await _http.GetFromJsonAsync<UserProfilesResponse>(query.ToString(), ct);
        return resp?.Data ?? [];
    }

    public async Task<IEnumerable<CoachProfileResponse>> GetAllProfilesAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<UserProfilesResponse>("api/CoachProfile/all", ct);
        return resp?.Data ?? [];
    }

    public async Task<bool> ActivateAsync(string userId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync($"api/Auth/coaches/{Uri.EscapeDataString(userId)}/activate", content: null, ct);
        var body = await resp.Content.ReadFromJsonAsync<IdentityGenericResponse>(ct);
        return resp.IsSuccessStatusCode && (body?.Data ?? false);
    }
}
