// =============================================================================
// File           : AuthorizationPolicies.cs
// Layer          : Api / Authorization
// Purpose        : Names of the role policies used by the endpoints.
// Logic          : Role-level rules ("only a patient", "only a doctor") are declared once as named policies and
//                  referenced by name; the resource-level rule ("this particular patient") is PatientAccessRequirement.
// Implementation : String constants plus the registration helper used by AddApi.
// =============================================================================
using CardioSense.Domain.Enums;
using CardioSense.Infrastructure.Identity;

namespace CardioSense.Api.Authorization;

/// <summary>Named authorization policies.</summary>
internal static class AuthorizationPolicies
{
    /// <summary>The caller is a patient and carries a patient profile id.</summary>
    public const string PatientOnly = "PatientOnly";

    /// <summary>The caller is a doctor and carries a doctor profile id.</summary>
    public const string DoctorOnly = "DoctorOnly";

    /// <summary>Registers the role policies.</summary>
    public static IServiceCollection AddRolePolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(PatientOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(UserRole.Patient))
                .RequireClaim(JwtClaimNames.PatientId))
            .AddPolicy(DoctorOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(UserRole.Doctor))
                .RequireClaim(JwtClaimNames.DoctorId));

        return services;
    }
}
