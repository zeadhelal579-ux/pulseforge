// =============================================================================
// File           : AuthDtos.cs
// Layer          : Application / Auth/Common
// Purpose        : Auth response contracts: the signed-in user, the login/signup response and the refresh
//                  response.
// Logic          : These are the exact bodies the frontend expects (AuthenticatedUser, AuthResponse,
//                  RefreshResponse); the refresh token is never part of a body - it travels in the httpOnly
//                  cookie.
// Implementation : Immutable positional records declared together because they form one auth contract.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Auth.Common;

/// <summary>The signed-in user (Id is the account id; PatientId/DoctorId are profile ids).</summary>
public sealed record AuthenticatedUserDto(
    Guid Id,
    UserRole Role,
    string FullName,
    string Email,
    string PreferredLanguage,
    Guid? PatientId,
    Guid? DoctorId,
    string? Specialty);

/// <summary>Body of the login and signup responses.</summary>
public sealed record AuthResponse(string AccessToken, AuthenticatedUserDto User);

/// <summary>Body of the refresh response.</summary>
public sealed record RefreshResponse(string AccessToken);
