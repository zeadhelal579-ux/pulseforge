// =============================================================================
// File           : AppDbContext.cs
// Layer          : Infrastructure / Persistence
// Purpose        : The EF Core database context - implements IAppDbContext and hosts the Identity tables.
// Logic          : Application handlers see only IAppDbContext (one DbSet per entity, no repositories). The context
//                  also carries the login tables (Users, UserClaims, UserLogins, UserTokens) and the vitals table,
//                  which is reachable only through IVitalsTimeSeriesStore so it can move to another database later.
// Implementation : IdentityUserContext (users without the unused role tables - roles are a column on Users).
//                  Every mapping lives in a configuration class; this file only renames the Identity tables and
//                  declares the patient file-number sequence.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Identity;
using CardioSense.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CardioSense.Infrastructure.Persistence;

/// <summary>The relational database of CardioSense.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options), IAppDbContext
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

    /// <summary>Vitals readings; used only by the SQL time-series store (not part of IAppDbContext).</summary>
    public DbSet<VitalsSample> VitalsSamples => Set<VitalsSample>();

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        Database.BeginTransactionAsync(ct);

    /// <summary>Builds the model: Identity tables first, then every configuration class of this assembly.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        modelBuilder.HasSequence<int>(PatientConfiguration.FileNumberSequence).StartsAt(1).IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
