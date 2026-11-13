// =============================================================================
// File           : ICurrentUser.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Who is making the current request (account, role and profile ids).
// Logic          : Handlers never touch HttpContext; they ask this interface. Profile ids come from JWT
//                  claims added at login.
// Implementation : Implemented in the Api layer from the authenticated ClaimsPrincipal; every member is null
//                  when unauthenticated.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Interfaces;

/// <summary>The authenticated caller.</summary>
public interface ICurrentUser
{
    /// <summary>The login account id (JWT 'sub'); null when unauthenticated.</summary>
    Guid? UserId { get; }

    /// <summary>The caller's role; null when unauthenticated.</summary>
    UserRole? Role { get; }

    /// <summary>The caller's patient profile id when the role is Patient.</summary>
    Guid? PatientId { get; }

    /// <summary>The caller's doctor profile id when the role is Doctor.</summary>
    Guid? DoctorId { get; }
}
