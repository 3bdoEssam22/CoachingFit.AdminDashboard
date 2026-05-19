using System.Security.Claims;
using CoachingFit.Identity.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Components.Authorization;

namespace CoachingFit.AdminDashboard.Services;

public class CircuitAuthenticationStateProvider(TokenStore _tokenStore) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_tokenStore.IsSignedIn || _tokenStore.Auth is null)
            return Task.FromResult(Anonymous);

        return Task.FromResult(new AuthenticationState(BuildPrincipal(_tokenStore.Auth)));
    }

    public void SignIn(AuthResponse auth)
    {
        _tokenStore.Set(auth);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(BuildPrincipal(auth))));
    }

    public void SignOut()
    {
        _tokenStore.Clear();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static ClaimsPrincipal BuildPrincipal(AuthResponse auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.UserId),
            new(ClaimTypes.Name, auth.FullName),
            new(ClaimTypes.Email, auth.Email),
            new(ClaimTypes.Role, auth.Role),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Circuit"));
    }
}
