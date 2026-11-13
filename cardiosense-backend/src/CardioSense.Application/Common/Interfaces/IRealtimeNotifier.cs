// =============================================================================
// File           : IRealtimeNotifier.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Push channel to browsers (SignalR): live vitals, ECG samples, risk status, new alerts,
//                  doctor list summaries.
// Logic          : Event names the frontend listens to: vitals:{patientId}, ecg:{patientId},
//                  riskStatus:{patientId}, alert:{patientId}, patientSummary:{doctorId}. Risk status is
//                  broadcast in two variants so patients never receive doctor-only numbers.
// Implementation : Implemented by SignalRRealtimeNotifier in Infrastructure. Delivery is best-effort:
//                  implementations must log and swallow transport errors so a push failure never fails the
//                  use case.
// =============================================================================
using CardioSense.Application.Common.Dtos;

namespace CardioSense.Application.Common.Interfaces;

/// <summary>Real-time push to connected clients.</summary>
public interface IRealtimeNotifier
{
    /// <summary>Publishes a heart-rate / SpO2 tick to everyone watching the patient (vitals:{patientId}).</summary>
    Task NotifyVitalsAsync(VitalsTickPayload payload, CancellationToken ct = default);

    /// <summary>Publishes ECG samples in order, one message per sample (ecg:{patientId}); never persisted.</summary>
    Task NotifyEcgSamplesAsync(Guid patientId, IReadOnlyList<float> samples, CancellationToken ct = default);

    /// <summary>
    /// Publishes a status change: clinical variant to doctors, patient-safe variant to the patient
    /// (riskStatus:{patientId}).
    /// </summary>
    Task NotifyRiskStatusAsync(RiskStatusBroadcast broadcast, CancellationToken ct = default);

    /// <summary>Publishes a newly raised alert (alert:{patientId}).</summary>
    Task NotifyAlertAsync(AlertDto alert, CancellationToken ct = default);

    /// <summary>Publishes a light summary to each doctor's list screen (patientSummary:{doctorId}).</summary>
    Task NotifyPatientSummaryAsync(
        IReadOnlyCollection<Guid> doctorIds, PatientSummaryUpdate update, CancellationToken ct = default);
}
