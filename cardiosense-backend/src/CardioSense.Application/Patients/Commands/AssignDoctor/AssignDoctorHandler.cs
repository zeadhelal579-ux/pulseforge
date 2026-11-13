// =============================================================================
// File           : AssignDoctorHandler.cs
// Layer          : Application / Patients/Commands/AssignDoctor
// Purpose        : Handles adding a patient to the doctor's caseload.
// Logic          : Looks the patient up by national id, creates the doctor-patient assignment (primary when
//                  the patient has no primary doctor yet) and returns the patient's list row. Adding a
//                  patient who is already assigned simply returns the row (idempotent).
// Implementation : Internal sealed handler. Privacy note: a national id is not a secret, so this path should
//                  later require patient consent (invite code).
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Doctors.Common;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Commands.AssignDoctor;

/// <summary>Handles <see cref="AssignDoctorCommand"/>.</summary>
internal sealed class AssignDoctorHandler(
    IAppDbContext db, ICurrentUser currentUser, PatientListItemAssembler assembler, TimeProvider clock)
    : IRequestHandler<AssignDoctorCommand, Result<PatientListItemDto>>
{
    /// <summary>Links the patient to the doctor and returns the list row.</summary>
    public async ValueTask<Result<PatientListItemDto>> Handle(AssignDoctorCommand request, CancellationToken ct)
    {
        if (currentUser.DoctorId is not { } doctorId)
        {
            return Result.Failure<PatientListItemDto>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var nationalId = request.Identifier.Trim();
        var patientId = await db.Patients.AsNoTracking()
            .Where(p => p.NationalId == nationalId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct);

        if (patientId is not { } id)
        {
            return Result.Failure<PatientListItemDto>(ApplicationErrors.Patient.NotFound);
        }

        var alreadyAssigned = await db.DoctorPatientAssignments
            .AnyAsync(a => a.DoctorId == doctorId && a.PatientId == id, ct);

        if (!alreadyAssigned)
        {
            var hasPrimaryDoctor = await db.DoctorPatientAssignments.AnyAsync(a => a.PatientId == id && a.IsPrimary,
                ct);
            db.DoctorPatientAssignments.Add(
                DoctorPatientAssignment.Create(doctorId, id, !hasPrimaryDoctor, clock.GetUtcNow()));
            await db.SaveChangesAsync(ct);
        }

        var rows = await assembler.BuildAsync([id], ct);
        return Result.Success(rows.Single());
    }
}
