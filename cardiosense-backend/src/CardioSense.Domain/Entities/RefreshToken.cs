// =============================================================================
// File           : RefreshToken.cs
// Layer          : Domain / Entities
// Purpose        : Server-side record of an issued refresh token (stored hashed, never in plaintext).
// Logic          : Tokens are rotated on every use: the old one is revoked the moment a new one is issued, so
//                  a leaked token that is replayed later is detectable. A short leeway tolerates two
//                  simultaneous refresh calls (two tabs, React StrictMode) that legitimately carry the same
//                  cookie.
// Implementation : Sealed entity; the checks take the current time and the leeway so they stay deterministic
//                  and testable.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Domain.Entities;

/// <summary>A refresh token record.</summary>
public sealed class RefreshToken : Entity
{
    /// <summary>Maximum length of the stored hash (SHA-256 in Base64 is 44 characters).</summary>
    public const int TokenHashMaxLength = 128;

    /// <summary>Used by EF Core.</summary>
    private RefreshToken()
    {
    }

    /// <summary>Builds a token record after guarding every value.</summary>
    private RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now)
        : base(now)
    {
        UserId = RequireId(userId, nameof(userId));
        TokenHash = RequireText(tokenHash, nameof(tokenHash), TokenHashMaxLength);
        ExpiresAt = expiresAt;
    }

    /// <summary>The account the token belongs to.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Hash of the token value (the raw value only ever lives in the httpOnly cookie).</summary>
    public string TokenHash { get; private set; } = null!;

    /// <summary>When the token stops being valid.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>When the token was revoked; null while it is still usable.</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Creates a new token record.</summary>
    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now) =>
        new(userId, tokenHash, expiresAt, now);

    /// <summary>
    /// True when the token has not expired and is either unrevoked or was revoked only moments ago (a concurrent
    /// retry).
    /// </summary>
    public bool IsUsable(DateTimeOffset now, TimeSpan reuseLeeway) =>
        ExpiresAt > now && (RevokedAt is null || now - RevokedAt.Value <= reuseLeeway);

    /// <summary>
    /// True when the token was revoked longer ago than the leeway - presenting it again signals replay or theft.
    /// </summary>
    public bool IsReplay(DateTimeOffset now, TimeSpan reuseLeeway) =>
        RevokedAt is { } revokedAt && now - revokedAt > reuseLeeway;

    /// <summary>Revokes the token (idempotent - the first revocation time is kept).</summary>
    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
