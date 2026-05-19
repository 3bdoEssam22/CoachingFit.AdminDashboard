using System.Net.Http.Json;
using CoachingFit.User.Shared.DTOs.Requests;
using CoachingFit.User.Shared.DTOs.Responses;
using UserBoolResponse = CoachingFit.User.Shared.Wrappers.GenericResponse<bool>;
using UserCertListResponse = CoachingFit.User.Shared.Wrappers.GenericResponse<System.Collections.Generic.IEnumerable<CoachingFit.User.Shared.DTOs.Responses.AdminCertificateResponse>>;

namespace CoachingFit.AdminDashboard.Services;

public class CertificateApiClient(HttpClient _http)
{
    public async Task<IEnumerable<AdminCertificateResponse>> GetForCoachAsync(
        string coachUserId, CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<UserCertListResponse>(
            $"api/CoachCertificate/coach/{Uri.EscapeDataString(coachUserId)}", ct);
        return resp?.Data ?? [];
    }

    public async Task<(bool ok, string? message)> ApproveAsync(Guid certificateId, CancellationToken ct = default)
    {
        var resp = await _http.PutAsync(
            $"api/CoachCertificate/{certificateId}/approve", content: null, ct);
        var body = await resp.Content.ReadFromJsonAsync<UserBoolResponse>(ct);
        return (resp.IsSuccessStatusCode && (body?.Data ?? false), body?.Message);
    }

    public async Task<(bool ok, string? message)> RejectAsync(
        Guid certificateId, RejectCertificateRequest request, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync(
            $"api/CoachCertificate/{certificateId}/reject", request, ct);
        var body = await resp.Content.ReadFromJsonAsync<UserBoolResponse>(ct);
        return (resp.IsSuccessStatusCode && (body?.Data ?? false), body?.Message);
    }
}
