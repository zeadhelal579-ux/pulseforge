// =============================================================================
// File           : MedicalHistoryEntry.cs
// Layer          : Domain / Entities
// Purpose        : One row of a patient's medical history (diagnosis, medication or condition).
// Logic          : Kept as discrete rows (not one text blob) so the UI can list and edit entries individually
//                  later.
// Implementation : Sealed entity with a static factory.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Domain.Entities;

/// <summary>A medical history row.</summary>
public sealed class MedicalHistoryEntry : Entity
{
    /// <summary>Maximum length of the description.</summary>
    public const int DescriptionMaxLength = 2000;

    /// <summary>Used by EF Core.</summary>
    private MedicalHistoryEntry()
    {
    }

    /// <summary>Builds an entry after guarding every value.</summary>
    private MedicalHistoryEntry(
        Guid patientId, MedicalHistoryEntryType entryType, string description, DateTimeOffset recordedAt)
        : base(recordedAt)
    {
        PatientId = RequireId(patientId, nameof(patientId));
        EntryType = entryType;
        Description = RequireText(description, nameof(description), DescriptionMaxLength);
        RecordedAt = recordedAt;
    }

    /// <summary>The patient the entry belongs to.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>Diagnosis, medication or condition.</summary>
    public MedicalHistoryEntryType EntryType { get; private set; }

    /// <summary>Free-text description.</summary>
    public string Description { get; private set; } = null!;

    /// <summary>When the entry was recorded.</summary>
    public DateTimeOffset RecordedAt { get; private set; }

    /// <summary>Creates a new entry.</summary>
    public static MedicalHistoryEntry Create(
        Guid patientId, MedicalHistoryEntryType entryType, string description, DateTimeOffset recordedAt) =>
        new(patientId, entryType, description, recordedAt);
}
