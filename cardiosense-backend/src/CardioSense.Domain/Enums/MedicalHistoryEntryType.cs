// =============================================================================
// File           : MedicalHistoryEntryType.cs
// Layer          : Domain / Enums
// Purpose        : Kind of a medical history row.
// Logic          : Diagnosis and Condition describe the patient's history; Medication lists current
//                  medicines.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>Kind of a medical history row.</summary>
public enum MedicalHistoryEntryType
{
    /// <summary>A diagnosed disease.</summary>
    Diagnosis = 0,

    /// <summary>A current medication.</summary>
    Medication = 1,

    /// <summary>A related condition.</summary>
    Condition = 2
}
