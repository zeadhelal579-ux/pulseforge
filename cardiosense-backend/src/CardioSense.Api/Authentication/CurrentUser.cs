// =============================================================================
// File           : CurrentUser.cs
// Layer          : Api / Authentication
// Purpose        : Implements ICurrentUser: who is calling, read from the validated access-token claims.
// Logic          : Application and Infrastructure never touch HttpContext; they ask ICurrentUser. Claims are read by
//                  their raw names (inbound claim mapping is switched off in JwtBearerSetup) from JwtClaimNames, the
//                  same constants the token service used to write them. SignalR hub calls do not reliably have an
//                  HttpContext, so a hub filter can hand the connection's user over explicitly (UsePrincipal).
// Implementation : Scoped, one instance per request or per hub invocation. A missing or malformed claim yields null,
//                  never an exception - unauthenticated callers simply have no ids.
// =============================================================================
using System.Security.Claims;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Enums;
using CardioSense.Infrastructure.Identity;

namespace CardioSense.Api.Authentication;

/// <summary>The authenticated caller, taken from the access-token claims.</summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    /// <summary>User supplied by a hub filter; takes precedence over the HTTP context.</summary>
    private ClaimsPrincipal? _hubPrincipal;

    /// <inheritdoc />
    public Guid? UserId => ReadGuid(JwtClaimNames.Subject);

    /// <inheritdoc />
    public UserRole? Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirst(JwtClaimNames.Role)?.Value, out var role) ? role : null;

    /// <inheritdoc />
    public Guid? PatientId => ReadGuid(JwtClaimNames.PatientId);

    /// <inheritdoc />
    public Guid? DoctorId => ReadGuid(JwtClaimNames.DoctorId);

    /// <summary>Uses the given user instead of the HTTP context (SignalR hub invocations).</summary>
    public void UsePrincipal(ClaimsPrincipal principal) => _hubPrincipal = principal;

    /// <summary>The user to read claims from.</summary>
    private ClaimsPrincipal? Principal => _hubPrincipal ?? httpContextAccessor.HttpContext?.User;

    /// <summary>Reads a claim as a Guid, or null when it is absent or not a Guid.</summary>
    private Guid? ReadGuid(string claimType) =>
        Guid.TryParse(Principal?.FindFirst(claimType)?.Value, out var id) ? id : null;
}
