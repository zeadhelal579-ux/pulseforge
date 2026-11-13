// =============================================================================
// File           : MonitoringHub.cs
// Layer          : Infrastructure / Realtime
// Purpose        : The SignalR hub (/hubs/monitoring) browsers connect to for live ECG, vitals, status and alerts.
// Logic          : On connect a patient joins their own group and a doctor joins their personal group, both taken from
//                  the token claims. A doctor watches one patient at a time by calling JoinPatientGroup (and
//                  LeavePatientGroup when leaving the detail page) - never all patients at once. Every join goes
//                  through IPatientAccessChecker, the single place that decides who may see which patient.
// Implementation : Lives in Infrastructure (not Api) because the notifier needs the hub type and Infrastructure must
//                  not reference Api; the Api only maps it. Requires an authenticated connection.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CardioSense.Infrastructure.Realtime;

/// <summary>Real-time monitoring hub.</summary>
[Authorize]
public sealed class MonitoringHub(IPatientAccessChecker access) : Hub
{
    /// <summary>Puts the new connection into the personal group that matches the caller's role.</summary>
    public override async Task OnConnectedAsync()
    {
        if (TryReadProfileId(JwtClaimNames.PatientId, out var patientId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, MonitoringNames.PatientGroup(patientId));
        }

        if (TryReadProfileId(JwtClaimNames.DoctorId, out var doctorId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, MonitoringNames.DoctorGroup(doctorId));
        }

        await base.OnConnectedAsync();
    }

    /// <summary>Starts receiving live data of a patient; refused unless the caller may access that patient.</summary>
    public async Task JoinPatientGroup(Guid patientId)
    {
        if (!await access.CanAccessAsync(patientId, Context.ConnectionAborted))
        {
            throw new HubException("You are not allowed to watch this patient.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ViewerGroupOf(patientId), Context.ConnectionAborted);
    }

    /// <summary>Stops receiving live data of a patient (called when the detail page closes).</summary>
    public Task LeavePatientGroup(Guid patientId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ViewerGroupOf(patientId), Context.ConnectionAborted);

    /// <summary>The group a viewer joins: doctors share the doctors group, a patient uses their own group.</summary>
    private string ViewerGroupOf(Guid patientId) =>
        TryReadProfileId(JwtClaimNames.DoctorId, out _)
            ? MonitoringNames.PatientDoctorsGroup(patientId)
            : MonitoringNames.PatientGroup(patientId);

    /// <summary>Reads a profile id from the token claims; false when the claim is missing or not a Guid.</summary>
    private bool TryReadProfileId(string claimType, out Guid profileId) =>
        Guid.TryParse(Context.User?.FindFirst(claimType)?.Value, out profileId);
}
