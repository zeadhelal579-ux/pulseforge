// =============================================================================
// File           : AuditLogEntryConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps AuditLogEntry to the AuditLogs table.
// Logic          : Answers "who looked at this record, and when" - so the index is (EntityType, EntityId, Timestamp).
//                  UserId is optional (unauthenticated attempts are logged too) and an audit row is never deleted
//                  with its account.
// Implementation : IEntityTypeConfiguration picked up by ApplyConfigurationsFromAssembly.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="AuditLogEntry"/>.</summary>
public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ConfigureEntity("AuditLogs");

        builder.Property(entry => entry.Action).HasMaxLength(AuditLogEntry.TextMaxLength);
        builder.Property(entry => entry.EntityType).HasMaxLength(AuditLogEntry.TextMaxLength);
        builder.Property(entry => entry.IpAddress).HasMaxLength(AuditLogEntry.IpAddressMaxLength);

        builder.HasIndex(entry => new { entry.EntityType, entry.EntityId, entry.Timestamp });

        builder.HasRestrictedForeignKey<AuditLogEntry, ApplicationUser>(entry => entry.UserId);
    }
}
