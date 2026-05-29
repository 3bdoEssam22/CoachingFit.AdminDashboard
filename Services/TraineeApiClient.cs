using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoachingFit.Identity.Shared.DTOs.Responses;
using CoachingFit.User.Shared.DTOs.Responses;
using IdentityIdsResponse = CoachingFit.Identity.Shared.Wrappers.GenericResponse<System.Collections.Generic.IEnumerable<string>>;
using IdentityTraineeDetailsResponse = CoachingFit.Identity.Shared.Wrappers.GenericResponse<System.Collections.Generic.IEnumerable<CoachingFit.Identity.Shared.DTOs.Responses.TraineeUserSummary>>;
using UserTraineeResponse = CoachingFit.User.Shared.Wrappers.GenericResponse<CoachingFit.User.Shared.DTOs.Responses.TraineeProfileResponse>;
using UserTraineesResponse = CoachingFit.User.Shared.Wrappers.GenericResponse<System.Collections.Generic.IEnumerable<CoachingFit.User.Shared.DTOs.Responses.TraineeProfileResponse>>;

namespace CoachingFit.AdminDashboard.Services;

public class TraineeApiClient
{
    private readonly HttpClient _http;

    public TraineeApiClient(HttpClient http, TokenStore tokenStore)
    {
        _http = http;
        if (tokenStore.AccessToken is { } token)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<IEnumerable<string>> GetAllUserIdsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<IdentityIdsResponse>("api/Auth/trainees/all", ct);
        return resp?.Data ?? [];
    }

    public async Task<IEnumerable<TraineeProfileResponse>> GetAllProfilesAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<UserTraineesResponse>("api/TraineeProfile/all", ct);
        return resp?.Data ?? [];
    }

    public async Task<TraineeProfileResponse?> GetByIdAsync(Guid profileId, CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<UserTraineeResponse>(
            $"api/TraineeProfile/{profileId}", ct);
        return resp?.Data;
    }

    public async Task<IEnumerable<TraineeUserSummary>> GetTraineeDetailsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<IdentityTraineeDetailsResponse>("api/Auth/trainees/details", ct);
        return resp?.Data ?? [];
    }
}
