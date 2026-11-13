// =============================================================================
// File           : ElectrodeVerificationConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps ElectrodeVerification to the ElectrodeVerifications table.
// Logic          : Results of the camera-based electrode placement check; the dashboard reads the newest one per
//                  patient, so (PatientId, Timestamp) is indexed.
// Implementation : IEntityTypeConfiguration picked up by ApplyConfigurationsFromAssembly.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="ElectrodeVerification"/>.</summary>
public sealed class ElectrodeVerificationConfiguration : IEntityTypeConfiguration<ElectrodeVerification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ElectrodeVerification> builder)
    {
        builder.ConfigureEntity("ElectrodeVerifications");

        builder.Property(verification => verification.PlacementStatus).HasEnumAsString();

        builder.HasIndex(verification => new { verification.PatientId, verification.Timestamp });

        builder.HasRestrictedForeignKey<ElectrodeVerification, Patient>(verification => verification.PatientId);
    }
}
