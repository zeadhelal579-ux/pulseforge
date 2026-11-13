// =============================================================================
// File           : MonitoringNames.cs
// Layer          : Infrastructure / Realtime
// Purpose        : The single source of truth for SignalR group names and client event names.
// Logic          : Per patient there are two audiences: the patient's own connections (patient-{id}) and the doctors
//                  currently viewing the patient (patient-{id}-doctors) - they receive different variants of the risk
//                  status. Each doctor also has a personal group (doctor-{id}) for the patient-list summaries. Event
//                  names follow the frontend contract: vitals:{id}, ecg:{id}, riskStatus:{id}, alert:{id},
//                  patientSummary:{doctorId}.
// Implementation : Internal static string builders so the hub and the notifier can never disagree on a name.
// =============================================================================
namespace CardioSense.Infrastructure.Realtime;

/// <summary>Group and event names used by the monitoring hub.</summary>
internal static class MonitoringNames
{
    /// <summary>Group of the patient's own connections.</summary>
    public static string PatientGroup(Guid patientId) => $"patient-{patientId}";

    /// <summary>Group of the doctors who are viewing the patient's detail page.</summary>
    public static string PatientDoctorsGroup(Guid patientId) => $"patient-{patientId}-doctors";

    /// <summary>Group of one doctor's own connections (patient-list summaries).</summary>
    public static string DoctorGroup(Guid doctorId) => $"doctor-{doctorId}";

    /// <summary>Event carrying a heart-rate / SpO2 tick.</summary>
    public static string VitalsEvent(Guid patientId) => $"vitals:{patientId}";

    /// <summary>Event carrying one ECG waveform sample.</summary>
    public static string EcgEvent(Guid patientId) => $"ecg:{patientId}";

    /// <summary>Event carrying a risk status change.</summary>
    public static string RiskStatusEvent(Guid patientId) => $"riskStatus:{patientId}";

    /// <summary>Event carrying a newly raised alert.</summary>
    public static string AlertEvent(Guid patientId) => $"alert:{patientId}";

    /// <summary>Event carrying a patient-list summary for one doctor.</summary>
    public static string PatientSummaryEvent(Guid doctorId) => $"patientSummary:{doctorId}";
}
