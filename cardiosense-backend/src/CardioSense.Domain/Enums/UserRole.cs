// =============================================================================
// File           : UserRole.cs
// Layer          : Domain / Enums
// Purpose        : Role of an authenticated account.
// Logic          : Drives authorization and decides which profile table (Patients or Doctors) belongs to the
//                  account.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>Role of an authenticated account.</summary>
public enum UserRole
{
    /// <summary>A monitored patient.</summary>
    Patient = 0,

    /// <summary>A treating doctor.</summary>
    Doctor = 1
}
