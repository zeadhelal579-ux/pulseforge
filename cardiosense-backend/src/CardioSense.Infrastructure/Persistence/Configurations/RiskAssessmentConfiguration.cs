// =============================================================================
// File           : RiskAssessmentConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps RiskAssessment to the RiskAssessments table.
// Logic          : One assessment per feature set at most (a window the AI could not assess has none). The doctor-
//                  facing flags are a list in the entity and one JSON text column (ContributingFactorsJson) in the
//                  table; a value comparer tells EF Core how to detect changes inside the list.
// Implementation : ValueConverter + ValueComparer built from small static helpers (expression trees cannot hold
//                  collection expressions); (PatientId, Timestamp) is indexed for "latest" and timeline queries.
// =============================================================================
using System.Text.Json;
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="RiskAssessment"/>.</summary>
public sealed class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    /// <summary>Longest stored flag list (JSON text).</summary>
    private const int FlagsMaxLength = 2000;

    /// <summary>Converts the flag list to JSON text and back.</summary>
    private static readonly ValueConverter<IReadOnlyList<string>, string> FlagsConverter = new(
        flags => SerializeFlags(flags),
        json => DeserializeFlags(json));

    /// <summary>Compares flag lists by content so EF Core notices edits and takes correct snapshots.</summary>
    private static readonly ValueComparer<IReadOnlyList<string>> FlagsComparer = new(
        (left, right) => AreSameFlags(left, right),
        flags => HashFlags(flags),
        flags => CopyFlags(flags));

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.ConfigureEntity("RiskAssessments");

        builder.Property(assessment => assessment.StatusLevel).HasEnumAsString();
        builder.Property(assessment => assessment.ModelVersion).HasMaxLength(RiskAssessment.ModelVersionMaxLength);
        builder.Property(assessment => assessment.Flags)
            .HasColumnName("ContributingFactorsJson")
            .HasMaxLength(FlagsMaxLength)
            .HasConversion(FlagsConverter, FlagsComparer);

        builder.HasIndex(assessment => new { assessment.PatientId, assessment.Timestamp });

        builder.HasRestrictedForeignKey<RiskAssessment, Patient>(assessment => assessment.PatientId);
        builder.HasOne<EcgFeatureSet>()
            .WithOne()
            .HasForeignKey<RiskAssessment>(assessment => assessment.EcgFeatureSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>Writes the flags as a JSON array.</summary>
    private static string SerializeFlags(IReadOnlyList<string> flags) => JsonSerializer.Serialize(flags);

    /// <summary>Reads a JSON array of flags; empty or unreadable content becomes an empty list.</summary>
    private static IReadOnlyList<string> DeserializeFlags(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];

    /// <summary>True when both lists hold the same flags in the same order.</summary>
    private static bool AreSameFlags(IReadOnlyList<string>? left, IReadOnlyList<string>? right) =>
        ReferenceEquals(left, right) || (left is not null && right is not null && left.SequenceEqual(right));

    /// <summary>Hash code that depends on the content of the list.</summary>
    private static int HashFlags(IReadOnlyList<string> flags) =>
        flags.Aggregate(0, (hash, flag) => HashCode.Combine(hash, flag));

    /// <summary>Independent copy used as the change-tracking snapshot.</summary>
    private static IReadOnlyList<string> CopyFlags(IReadOnlyList<string> flags) => flags.ToList();
}
