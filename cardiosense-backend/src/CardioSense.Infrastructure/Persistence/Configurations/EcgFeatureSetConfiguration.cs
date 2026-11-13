// =============================================================================
// File           : EcgFeatureSetConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps EcgFeatureSet to the EcgFeatureSets table.
// Logic          : Stores the processed features (PR, QRS, QT, QTc) - never the raw waveform. Queried as "latest per
//                  patient", so (PatientId, Timestamp) is indexed.
// Implementation : IEntityTypeConfiguration picked up by ApplyConfigurationsFromAssembly.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="EcgFeatureSet"/>.</summary>
public sealed class EcgFeatureSetConfiguration : IEntityTypeConfiguration<EcgFeatureSet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EcgFeatureSet> builder)
    {
        builder.ConfigureEntity("EcgFeatureSets");

        builder.Property(features => features.FeatureVectorJson).HasMaxLength(EcgFeatureSet.FeatureVectorMaxLength);

        builder.HasIndex(features => new { features.PatientId, features.Timestamp });

        builder.HasRestrictedForeignKey<EcgFeatureSet, Patient>(features => features.PatientId);
    }
}
