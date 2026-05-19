using CoachingFit.Identity.Shared.DTOs.Responses;

namespace CoachingFit.AdminDashboard.Services;

public class TokenStore
{
    public AuthResponse? Auth { get; private set; }

    public string? AccessToken => Auth?.Token;

    public bool IsSignedIn => !string.IsNullOrWhiteSpace(Auth?.Token);

    public void Set(AuthResponse auth) => Auth = auth;

    public void Clear() => Auth = null;
}
