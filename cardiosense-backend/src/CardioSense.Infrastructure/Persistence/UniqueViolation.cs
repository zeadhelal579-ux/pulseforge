// =============================================================================
// File           : UniqueViolation.cs
// Layer          : Infrastructure / Persistence
// Purpose        : Tells the Api whether a failed save was caused by a unique-index violation.
// Logic          : Unique indexes (national id, license number, serial number, device-per-patient, primary doctor)
//                  are the last line of defence against two requests racing past an application-level check. When
//                  that happens the right answer is a 409 Conflict, not a 500. Only Infrastructure knows which
//                  database error numbers mean "duplicate key", so the knowledge lives here and the Api asks.
// Implementation : SQL Server reports a duplicate unique index as error 2601 and a duplicate key constraint as 2627.
// =============================================================================
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Infrastructure.Persistence;

/// <summary>Recognises duplicate-key failures raised by the database.</summary>
public static class UniqueViolation
{
    /// <summary>SQL Server error number: duplicate row in a unique index.</summary>
    private const int DuplicateIndexRow = 2601;

    /// <summary>SQL Server error number: duplicate key in a unique or primary key constraint.</summary>
    private const int DuplicateKey = 2627;

    /// <summary>True when the exception is a failed save caused by a unique-index violation.</summary>
    public static bool IsCausedBy(Exception exception) =>
        exception is DbUpdateException { InnerException: SqlException { Number: DuplicateIndexRow or DuplicateKey } };
}
