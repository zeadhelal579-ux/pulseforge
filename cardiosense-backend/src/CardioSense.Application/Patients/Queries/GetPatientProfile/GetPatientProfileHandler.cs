// =============================================================================
// File           : GetPatientProfileHandler.cs
// Layer          : Application / Patients/Queries/GetPatientProfile
// Purpose        : Handles GetPatientProfileQuery.
// Logic          : Doctors receive the clinical risk details; patients receive the status only (server-side
//                  redaction).
// Implementation : Internal sealed handler; the heavy lifting is shared with UpdatePatientProfile through
//                  PatientProfileAssembler.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Patients.Common;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetPatientProfile;

/// <summary>Handles <see cref="GetPatientProfileQuery"/>.</summary>
internal sealed class GetPatientProfileHandler(PatientProfileAssembler assembler, ICurrentUser currentUser)
    : IRequestHandler<GetPatientProfileQuery, Result<PatientProfileDto>>
{
    /// <summary>Builds the aggregate for the requested patient.</summary>
    public async ValueTask<Result<PatientProfileDto>> Handle(GetPatientProfileQuery request, CancellationToken ct) =>
        await assembler.BuildAsync(request.PatientId, currentUser.Role == UserRole.Doctor, ct);
}
