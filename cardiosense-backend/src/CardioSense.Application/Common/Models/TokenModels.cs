// =============================================================================
// File           : TokenModels.cs
// Layer          : Application / Common/Models
// Purpose        : Data shapes exchanged with ITokenService (what goes into a JWT, the issued JWT, a fresh
//                  refresh token).
// Logic          : The raw refresh token value is returned once (to be set as an httpOnly cookie); only its
//                  hash is stored.
// Implementation : Three small immutable records declared together because they are used only by the token
//                  contract.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Models;

/// <summary>Claims to embed in an access token.</summary>
public sealed record AccessTokenRequest(Guid UserId, UserRole Role, Guid? PatientId, Guid? DoctorId);

/// <summary>An issued access token (JWT) and its expiry.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>A freshly generated refresh token: raw value for the cookie, hash for storage.</summary>
public sealed record GeneratedRefreshToken(string RawValue, string Hash, DateTimeOffset ExpiresAt);
