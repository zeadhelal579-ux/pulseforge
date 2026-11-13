// =============================================================================
// File           : VitalsSampleConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps VitalsSample to the VitalsSamples table (the Phase 1 home of the vitals time series).
// Logic          : Almost pure appends, read as "latest" or "a time window of one patient", so the one index that
//                  matters is (PatientId, Timestamp). If volume ever outgrows SQL Server only this table moves
//                  (behind IVitalsTimeSeriesStore).
// Implementation : IEntityTypeConfiguration; VitalsSamples is not exposed by IAppDbContext on purpose.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="VitalsSample"/>.</summary>
public sealed class VitalsSampleConfiguration : IEntityTypeConfiguration<VitalsSample>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VitalsSample> builder)
    {
        builder.ConfigureEntity("VitalsSamples");

        builder.HasIndex(sample => new { sample.PatientId, sample.Timestamp });

        builder.HasRestrictedForeignKey<VitalsSample, Patient>(sample => sample.PatientId);
        builder.HasRestrictedForeignKey<VitalsSample, Device>(sample => sample.DeviceId);
    }
}
