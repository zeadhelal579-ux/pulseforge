// =============================================================================
// File           : AlertConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps Alert to the Alerts table.
// Logic          : Two access paths drive the indexes: the alerts timeline (newest first per patient) and the
//                  "is a critical alert still open?" check that runs on every telemetry message.
// Implementation : Composite indexes (PatientId, TriggeredAt) and (PatientId, Severity, ResolvedAt).
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="Alert"/>.</summary>
public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ConfigureEntity("Alerts");

        builder.Property(alert => alert.Type).HasEnumAsString();
        builder.Property(alert => alert.Severity).HasEnumAsString();
        builder.Property(alert => alert.Message).HasMaxLength(Alert.MessageMaxLength);
        builder.Property(alert => alert.ResolutionNote).HasMaxLength(Alert.MessageMaxLength);

        builder.HasIndex(alert => new { alert.PatientId, alert.TriggeredAt });
        builder.HasIndex(alert => new { alert.PatientId, alert.Severity, alert.ResolvedAt });

        builder.HasRestrictedForeignKey<Alert, Patient>(alert => alert.PatientId);
    }
}
