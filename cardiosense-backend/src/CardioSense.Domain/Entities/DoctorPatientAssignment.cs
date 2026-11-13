// =============================================================================
// File           : DoctorPatientAssignment.cs
// Layer          : Domain / Entities
// Purpose        : Link between a doctor and a patient they supervise.
// Logic          : Authorization checks this table on every doctor-side patient request; a patient has at
//                  most one primary doctor.
// Implementation : Sealed entity; AssignedAt is kept explicit (as in the architecture doc) even though
//                  CreatedAt equals it today.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Domain.Entities;

/// <summary>A doctor-patient supervision link.</summary>
public sealed class DoctorPatientAssignment : Entity
{
    /// <summary>Used by EF Core.</summary>
    private DoctorPatientAssignment()
    {
    }

    /// <summary>Builds an assignment after guarding the ids.</summary>
    private DoctorPatientAssignment(Guid doctorId, Guid patientId, bool isPrimary, DateTimeOffset now)
        : base(now)
    {
        DoctorId = RequireId(doctorId, nameof(doctorId));
        PatientId = RequireId(patientId, nameof(patientId));
        IsPrimary = isPrimary;
        AssignedAt = now;
    }

    /// <summary>The supervising doctor.</summary>
    public Guid DoctorId { get; private set; }

    /// <summary>The supervised patient.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>When the link was created.</summary>
    public DateTimeOffset AssignedAt { get; private set; }

    /// <summary>True for the patient's primary doctor (the one the call button rings).</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>Creates a new assignment.</summary>
    public static DoctorPatientAssignment Create(Guid doctorId, Guid patientId, bool isPrimary, DateTimeOffset now) =>
        new(doctorId, patientId, isPrimary, now);
}
