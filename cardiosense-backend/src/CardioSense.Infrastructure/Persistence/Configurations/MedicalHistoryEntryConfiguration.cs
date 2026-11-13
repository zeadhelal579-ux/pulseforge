// =============================================================================
// File           : MedicalHistoryEntryConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps MedicalHistoryEntry to the MedicalHistoryEntries table.
// Logic          : One row per diagnosis / medication / condition so the UI can list and edit them individually.
// Implementation : The foreign-key index on PatientId is created by EF Core's convention.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="MedicalHistoryEntry"/>.</summary>
public sealed class MedicalHistoryEntryConfiguration : IEntityTypeConfiguration<MedicalHistoryEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MedicalHistoryEntry> builder)
    {
        builder.ConfigureEntity("MedicalHistoryEntries");

        builder.Property(entry => entry.EntryType).HasEnumAsString();
        builder.Property(entry => entry.Description).HasMaxLength(MedicalHistoryEntry.DescriptionMaxLength);

        builder.HasRestrictedForeignKey<MedicalHistoryEntry, Patient>(entry => entry.PatientId);
    }
}
