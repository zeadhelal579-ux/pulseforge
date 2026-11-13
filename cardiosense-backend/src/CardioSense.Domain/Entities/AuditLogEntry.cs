// =============================================================================
// File           : AuditLogEntry.cs
// Layer          : Domain / Entities
// Purpose        : One audit row: who touched which record, when and from where.
// Logic          : Written for every read/write of a patient record so 'who looked at this file' can be
//                  answered later (a real requirement once real medical data is stored).
// Implementation : Sealed, append-only entity; user and entity ids are nullable for anonymous or non-entity
//                  requests.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Domain.Entities;

/// <summary>An audit-trail row.</summary>
public sealed class AuditLogEntry : Entity
{
    /// <summary>Maximum length of the action and entity type texts.</summary>
    public const int TextMaxLength = 100;

    /// <summary>Maximum length of an IP address (IPv6 with scope).</summary>
    public const int IpAddressMaxLength = 64;

    /// <summary>Used by EF Core.</summary>
    private AuditLogEntry()
    {
    }

    /// <summary>Builds an audit row after guarding the texts.</summary>
    private AuditLogEntry(Guid? userId, string action, string entityType, Guid? entityId, string? ipAddress,
        DateTimeOffset now)
        : base(now)
    {
        UserId = userId;
        Action = RequireText(action, nameof(action), TextMaxLength);
        EntityType = RequireText(entityType, nameof(entityType), TextMaxLength);
        EntityId = entityId;
        IpAddress = RequireOptionalText(ipAddress, nameof(ipAddress), IpAddressMaxLength);
        Timestamp = now;
    }

    /// <summary>The account that performed the action; null when unauthenticated.</summary>
    public Guid? UserId { get; private set; }

    /// <summary>What was done (for example Read or Create).</summary>
    public string Action { get; private set; } = null!;

    /// <summary>Kind of record that was touched.</summary>
    public string EntityType { get; private set; } = null!;

    /// <summary>Id of the record that was touched, when there is one.</summary>
    public Guid? EntityId { get; private set; }

    /// <summary>When the action happened.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Caller IP address.</summary>
    public string? IpAddress { get; private set; }

    /// <summary>Creates an audit row.</summary>
    public static AuditLogEntry Create(
        Guid? userId, string action, string entityType, Guid? entityId, string? ipAddress, DateTimeOffset now) =>
        new(userId, action, entityType, entityId, ipAddress, now);
}
