using System.Net.Http.Headers;
using CoachingFit.AdminDashboard.Services;

namespace CoachingFit.AdminDashboard.Infrastructure;

public class BearerTokenHandler(TokenStore _tokenStore) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenStore.AccessToken;
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
