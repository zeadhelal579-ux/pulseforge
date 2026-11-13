// =============================================================================
// File           : AssignDoctorCommand.cs
// Layer          : Application / Patients/Commands/AssignDoctor
// Purpose        : Request for the signed-in doctor to add a patient to their caseload by national id.
// Logic          : Body of POST /api/doctors/me/patients ({identifier}) - the 'Add New Patient' button.
//                  Invite codes from the architecture doc are not supported yet (neither the schema nor the
//                  frontend has one), so only the national id resolves a patient.
// Implementation : Immutable record; returns the new list row so the UI can show it immediately.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Commands.AssignDoctor;

/// <summary>Add a patient to the signed-in doctor's caseload.</summary>
public sealed record AssignDoctorCommand(string Identifier) : IRequest<Result<PatientListItemDto>>;
