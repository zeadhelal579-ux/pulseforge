// =============================================================================
// File           : DoctorPatientAssignmentConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps DoctorPatientAssignment to the DoctorPatientAssignments table - the base of all access rules.
// Logic          : A doctor can supervise a patient only once; a patient has at most one primary doctor (the one the
//                  call button rings). Lookups run in both directions (a doctor's caseload, a patient's doctors).
// Implementation : Unique (DoctorId, PatientId) index, a plain PatientId index and a filtered unique index for the
//                  primary flag; the two PatientId indexes are named because they cover the same column.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="DoctorPatientAssignment"/>.</summary>
public sealed class DoctorPatientAssignmentConfiguration : IEntityTypeConfiguration<DoctorPatientAssignment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DoctorPatientAssignment> builder)
    {
        builder.ConfigureEntity("DoctorPatientAssignments");

        builder.HasIndex(assignment => new { assignment.DoctorId, assignment.PatientId }).IsUnique();
        builder.HasIndex(assignment => assignment.PatientId, "IX_DoctorPatientAssignments_PatientId");
        builder.HasIndex(assignment => assignment.PatientId, "UX_DoctorPatientAssignments_OnePrimaryPerPatient")
            .IsUnique()
            .HasFilter("[IsPrimary] = 1");

        builder.HasRestrictedForeignKey<DoctorPatientAssignment, Doctor>(assignment => assignment.DoctorId);
        builder.HasRestrictedForeignKey<DoctorPatientAssignment, Patient>(assignment => assignment.PatientId);
    }
}
