// =============================================================================
// File           : JwtClaimNames.cs
// Layer          : Infrastructure / Identity
// Purpose        : The single list of claim names stored in the access token.
// Logic          : The token writer (JwtTokenService), the SignalR hub and the Api's ICurrentUser must agree on these
//                  names, so they are declared once here instead of being repeated as string literals.
// Implementation : Public static class of constants; the values match the contract in backend-database-architecture.md
//                  section 8 (sub, role, plus the profile ids).
// =============================================================================
namespace CardioSense.Infrastructure.Identity;

/// <summary>Names of the claims inside the access token.</summary>
public static class JwtClaimNames
{
    /// <summary>The login account id.</summary>
    public const string Subject = "sub";

    /// <summary>The caller's role: Patient or Doctor.</summary>
    public const string Role = "role";

    /// <summary>The patient profile id (present for patients only).</summary>
    public const string PatientId = "patientId";

    /// <summary>The doctor profile id (present for doctors only).</summary>
    public const string DoctorId = "doctorId";
}
