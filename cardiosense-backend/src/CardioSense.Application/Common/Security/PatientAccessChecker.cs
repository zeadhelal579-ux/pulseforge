// =============================================================================
// File           : PatientAccessChecker.cs
// Layer          : Application / Common/Security
// Purpose        : Implements the access rule: a patient sees only themself, a doctor only patients assigned
//                  to them.
// Logic          : Patients are decided from claims alone (no database hit); doctors need one existence query
//                  against DoctorPatientAssignments. Unknown callers and unknown patients are denied.
// Implementation : Internal sealed class registered as IPatientAccessChecker; depends only on IAppDbContext
//                  and ICurrentUser.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Common.Security;

/// <summary>Decides whether the current caller may access a patient.</summary>
internal sealed class PatientAccessChecker(IAppDbContext db, ICurrentUser currentUser) : IPatientAccessChecker
{
    /// <inheritdoc />
    public async Task<bool> CanAccessAsync(Guid patientId, CancellationToken ct = default)
    {
        switch (currentUser.Role)
        {
            case UserRole.Patient:
                return currentUser.PatientId == patientId;

            case UserRole.Doctor when currentUser.DoctorId is { } doctorId:
                return await db.DoctorPatientAssignments
                    .AnyAsync(a => a.DoctorId == doctorId && a.PatientId == patientId, ct);

            default:
                return false;
        }
    }
}
