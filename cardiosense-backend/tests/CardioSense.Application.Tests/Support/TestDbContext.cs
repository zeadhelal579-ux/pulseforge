// =============================================================================
// File           : TestDbContext.cs
// Layer          : Tests / Support
// Purpose        : The in-memory implementation of IAppDbContext used by use-case tests.
// Logic          : Application only knows IAppDbContext, so tests need nothing from Infrastructure: a plain DbContext
//                  over the EF in-memory provider is enough. Entities are mapped by convention; only the list of AI
//                  flags needs an explicit converter because it is a collection.
// Implementation : DbSets mirror the interface; transactions are no-ops in the in-memory provider (the warning is
//                  silenced where the context is registered).
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CardioSense.Application.Tests.Support;

/// <summary>EF in-memory context implementing the application's database port.</summary>
public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IAppDbContext
{
    /// <inheritdoc />
    public DbSet<Patient> Patients => Set<Patient>();

    /// <inheritdoc />
    public DbSet<Doctor> Doctors => Set<Doctor>();

    /// <inheritdoc />
    public DbSet<Device> Devices => Set<Device>();

    /// <inheritdoc />
    public DbSet<DoctorPatientAssignment> DoctorPatientAssignments => Set<DoctorPatientAssignment>();

    /// <inheritdoc />
    public DbSet<MedicalHistoryEntry> MedicalHistoryEntries => Set<MedicalHistoryEntry>();

    /// <inheritdoc />
    public DbSet<EcgFeatureSet> EcgFeatureSets => Set<EcgFeatureSet>();

    /// <inheritdoc />
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    /// <inheritdoc />
    public DbSet<Alert> Alerts => Set<Alert>();

    /// <inheritdoc />
    public DbSet<ElectrodeVerification> ElectrodeVerifications => Set<ElectrodeVerification>();

    /// <inheritdoc />
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    /// <inheritdoc />
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        Database.BeginTransactionAsync(ct);

    /// <summary>Maps the AI flag list as a delimited string.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<RiskAssessment>().Property(risk => risk.Flags).HasConversion(
            flags => string.Join('|', flags),
            stored => stored.Split('|', StringSplitOptions.RemoveEmptyEntries));
}
