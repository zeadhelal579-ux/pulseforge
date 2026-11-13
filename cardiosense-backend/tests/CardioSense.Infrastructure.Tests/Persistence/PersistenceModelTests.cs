// =============================================================================
// File           : PersistenceModelTests.cs
// Layer          : Tests / Persistence
// Purpose        : Verifies the shape of the EF Core model without connecting to any database.
// Logic          : Building the model needs no connection, yet it reveals the mistakes that hurt most in production:
//                  a missing unique index (duplicate national ids), an accidental cascade delete (medical data wiped
//                  with its parent), enums stored as numbers (a reordered enum silently changes stored meaning).
//                  These are the rules the design promises, checked on every build.
// Implementation : Creates AppDbContext over the SQL Server provider with a connection string that is never used,
//                  then inspects the model.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Identity;
using CardioSense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CardioSense.Infrastructure.Tests.Persistence;

/// <summary>Tests for the AppDbContext model.</summary>
public sealed class PersistenceModelTests
{
    /// <summary>The model, built once (no database is contacted).</summary>
    private static readonly IModel Model = BuildModel();

    /// <summary>Builds the model from a context whose connection string is never opened.</summary>
    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var db = new AppDbContext(options);
        return db.Model;
    }

    /// <summary>True when the entity has a unique index over exactly the given properties.</summary>
    private static bool HasUniqueIndex(Type entity, params string[] properties)
    {
        var entityType = Model.FindEntityType(entity);
        Assert.NotNull(entityType);

        return entityType.GetIndexes().Any(index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(properties));
    }

    /// <summary>Every identity-like column is protected by a unique index.</summary>
    [Fact]
    public void Identity_columns_have_unique_indexes()
    {
        Assert.True(HasUniqueIndex(typeof(Patient), nameof(Patient.NationalId)));
        Assert.True(HasUniqueIndex(typeof(Patient), nameof(Patient.FileNumber)));
        Assert.True(HasUniqueIndex(typeof(Doctor), nameof(Doctor.LicenseNumber)));
        Assert.True(HasUniqueIndex(typeof(Device), nameof(Device.SerialNumber)));
        Assert.True(HasUniqueIndex(typeof(RefreshToken), nameof(RefreshToken.TokenHash)));
    }

    /// <summary>The database itself refuses a second device per patient and a repeated doctor assignment.</summary>
    [Fact]
    public void Pairing_and_assignment_rules_are_enforced_by_the_database()
    {
        Assert.True(HasUniqueIndex(typeof(Device), nameof(Device.PatientId)));
        Assert.True(HasUniqueIndex(
            typeof(DoctorPatientAssignment),
            nameof(DoctorPatientAssignment.DoctorId),
            nameof(DoctorPatientAssignment.PatientId)));
    }

    /// <summary>No foreign key between domain tables cascades a delete: medical data is never wiped silently.</summary>
    [Fact]
    public void Domain_foreign_keys_never_cascade_deletes()
    {
        var behaviours = Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == typeof(Patient).Namespace)
            .SelectMany(entityType => entityType.GetForeignKeys())
            .Select(foreignKey => foreignKey.DeleteBehavior)
            .Distinct()
            .ToList();

        Assert.Equal(DeleteBehavior.Restrict, Assert.Single(behaviours));
    }

    /// <summary>Enums are stored by name so reordering an enum cannot change what stored rows mean.</summary>
    [Fact]
    public void Enums_are_stored_as_text()
    {
        AssertStoredAsText(typeof(RiskAssessment), nameof(RiskAssessment.StatusLevel));
        AssertStoredAsText(typeof(Alert), nameof(Alert.Severity));
        AssertStoredAsText(typeof(Device), nameof(Device.ConnectionMode));
        AssertStoredAsText(typeof(Patient), nameof(Patient.Gender));
    }

    /// <summary>Tables carry the names used by the architecture document.</summary>
    [Fact]
    public void Tables_use_the_documented_names()
    {
        var expected = new Dictionary<Type, string>
        {
            [typeof(Patient)] = "Patients",
            [typeof(Doctor)] = "Doctors",
            [typeof(Device)] = "Devices",
            [typeof(DoctorPatientAssignment)] = "DoctorPatientAssignments",
            [typeof(MedicalHistoryEntry)] = "MedicalHistoryEntries",
            [typeof(VitalsSample)] = "VitalsSamples",
            [typeof(EcgFeatureSet)] = "EcgFeatureSets",
            [typeof(RiskAssessment)] = "RiskAssessments",
            [typeof(Alert)] = "Alerts",
            [typeof(ElectrodeVerification)] = "ElectrodeVerifications",
            [typeof(RefreshToken)] = "RefreshTokens",
            [typeof(AuditLogEntry)] = "AuditLogs",
            [typeof(ApplicationUser)] = "Users"
        };

        foreach (var (clrType, tableName) in expected)
        {
            var entityType = Model.FindEntityType(clrType);
            Assert.NotNull(entityType);
            Assert.Equal(tableName, entityType.GetTableName());
        }
    }

    /// <summary>The AI flags are stored as JSON in the documented column.</summary>
    [Fact]
    public void Risk_flags_are_stored_in_the_contributing_factors_column()
    {
        var property = Model.FindEntityType(typeof(RiskAssessment))?.FindProperty(nameof(RiskAssessment.Flags));

        Assert.NotNull(property);
        Assert.Equal("ContributingFactorsJson", property.GetColumnName());
    }

    /// <summary>Patient file numbers come from a database sequence, so two patients can never share one.</summary>
    [Fact]
    public void Patient_file_numbers_come_from_a_sequence()
    {
        Assert.NotNull(Model.FindSequence("PatientFileNumbers"));
    }

    /// <summary>Asserts that a property is stored as a string column.</summary>
    private static void AssertStoredAsText(Type entity, string propertyName)
    {
        var property = Model.FindEntityType(entity)?.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.GetProviderClrType());
    }
}
