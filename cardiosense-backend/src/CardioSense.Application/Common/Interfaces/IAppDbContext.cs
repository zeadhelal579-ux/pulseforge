// =============================================================================
// File           : IAppDbContext.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Abstraction of the relational database used by handlers (one DbSet per aggregate + save +
//                  transaction).
// Logic          : Handlers query EF Core directly through this interface instead of one repository per
//                  entity. Vitals are deliberately absent: they go through IVitalsTimeSeriesStore so the
//                  time-series storage can change later.
// Implementation : Implemented by AppDbContext in Infrastructure; exposing DbSet<T> keeps async LINQ
//                  available to handlers.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CardioSense.Application.Common.Interfaces;

/// <summary>Application view of the relational database.</summary>
public interface IAppDbContext
{
    /// <summary>Patient profiles.</summary>
    DbSet<Patient> Patients { get; }

    /// <summary>Doctor profiles.</summary>
    DbSet<Doctor> Doctors { get; }

    /// <summary>Devices (inventory and paired).</summary>
    DbSet<Device> Devices { get; }

    /// <summary>Doctor-patient supervision links.</summary>
    DbSet<DoctorPatientAssignment> DoctorPatientAssignments { get; }

    /// <summary>Medical history rows.</summary>
    DbSet<MedicalHistoryEntry> MedicalHistoryEntries { get; }

    /// <summary>Extracted ECG features.</summary>
    DbSet<EcgFeatureSet> EcgFeatureSets { get; }

    /// <summary>AI risk assessments.</summary>
    DbSet<RiskAssessment> RiskAssessments { get; }

    /// <summary>Clinical alerts.</summary>
    DbSet<Alert> Alerts { get; }

    /// <summary>Electrode placement checks.</summary>
    DbSet<ElectrodeVerification> ElectrodeVerifications { get; }

    /// <summary>Audit trail rows.</summary>
    DbSet<AuditLogEntry> AuditLogEntries { get; }

    /// <summary>Issued refresh tokens (hashed).</summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    /// <summary>Persists all pending changes in one unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Starts an explicit transaction for operations that must be atomic across several saves.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
