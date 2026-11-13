// =============================================================================
// File           : FreezeWaveformCommand.cs
// Layer          : Application / Patients/Commands/FreezeWaveform
// Purpose        : Request for a doctor to freeze or unfreeze the live waveform of a patient.
// Logic          : Body of POST /api/patients/{id}/freeze-waveform ({frozen}). Freezing is a presentation
//                  action in the doctor's browser; the call exists so the clinical action is recorded (audit
//                  middleware + log) and can later drive a shared freeze.
// Implementation : Immutable record implementing the patient-scope marker so access is enforced by the
//                  pipeline.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Commands.FreezeWaveform;

/// <summary>Freeze or unfreeze the waveform view.</summary>
public sealed record FreezeWaveformCommand(Guid PatientId, bool Frozen) : IRequest<Result>, IPatientScopedRequest;
