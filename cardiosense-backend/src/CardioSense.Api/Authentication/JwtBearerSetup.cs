// =============================================================================
// File           : JwtBearerSetup.cs
// Layer          : Api / Authentication
// Purpose        : Configures how incoming access tokens are validated.
// Logic          : Validation mirrors exactly what JwtTokenService writes: same issuer, audience, HS256 key and the
//                  raw claim names (inbound claim mapping is off, so "sub" stays "sub" and "role" stays "role").
//                  Browsers cannot set an Authorization header on a WebSocket, so for the monitoring hub only the
//                  token is also accepted from the access_token query string.
// Implementation : IConfigureNamedOptions fed by the already validated JwtOptions, so the signing key is read once
//                  and never duplicated in this project.
// =============================================================================
using System.Text;
using CardioSense.Api.Endpoints;
using CardioSense.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CardioSense.Api.Authentication;

/// <summary>Sets the JWT bearer validation rules from the token settings.</summary>
internal sealed class JwtBearerSetup(IOptions<JwtOptions> jwtOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    /// <summary>Allowed difference between server clocks when checking expiry.</summary>
    private static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(30);

    /// <summary>Configures the default bearer scheme.</summary>
    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);

    /// <summary>Configures the named bearer scheme (other schemes are left alone).</summary>
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        var settings = jwtOptions.Value;

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = true,
            ClockSkew = ClockSkew,
            NameClaimType = JwtClaimNames.Subject,
            RoleClaimType = JwtClaimNames.Role
        };
        options.Events = new JwtBearerEvents { OnMessageReceived = ReadHubToken };
    }

    /// <summary>Takes the token from the query string, but only for the SignalR hub path.</summary>
    private static Task ReadHubToken(MessageReceivedContext context)
    {
        var token = context.Request.Query["access_token"];
        var isHubRequest = context.HttpContext.Request.Path.StartsWithSegments(ApiRoutes.MonitoringHub);

        if (isHubRequest && !string.IsNullOrEmpty(token))
        {
            context.Token = token;
        }

        return Task.CompletedTask;
    }
}
