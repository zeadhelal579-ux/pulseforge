// =============================================================================
// File           : MonitoringOptions.cs
// Layer          : Application / Common/Options
// Purpose        : Tunable monitoring settings (doctor caseload capacity, how recent a device message must be
//                  to count as live).
// Logic          : Defaults work out of the box; the Api layer may override them from the 'Monitoring'
//                  configuration section.
// Implementation : Plain options class consumed through IOptions<MonitoringOptions>; DeviceLiveWindow is a
//                  computed convenience.
// =============================================================================
namespace CardioSense.Application.Common.Options;

/// <summary>Monitoring settings.</summary>
public sealed class MonitoringOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Monitoring";

    /// <summary>Capacity shown on the ward stats card ("42/50") - the doctor's monitored caseload size.</summary>
    public int DoctorPatientCapacity { get; set; } = 50;

    /// <summary>A device heard from within this many seconds is considered live.</summary>
    public int DeviceLiveWindowSeconds { get; set; } = 120;

    /// <summary>The live window as a TimeSpan.</summary>
    public TimeSpan DeviceLiveWindow => TimeSpan.FromSeconds(DeviceLiveWindowSeconds);
}
