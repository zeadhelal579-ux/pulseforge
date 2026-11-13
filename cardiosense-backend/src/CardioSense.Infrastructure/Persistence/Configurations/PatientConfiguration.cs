// =============================================================================
// File           : PatientConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps Patient to the Patients table.
// Logic          : National id, file number and the owning account are each unique. The file number (CS-0042) is
//                  produced by a database sequence on insert, so two simultaneous signups can never share one.
// Implementation : The sequence itself is declared in AppDbContext (sequences belong to the model, not an entity);
//                  its name is exposed here so both places agree.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="Patient"/>.</summary>
public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    /// <summary>Name of the SQL sequence that numbers patient files.</summary>
    public const string FileNumberSequence = "PatientFileNumbers";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ConfigureEntity("Patients");

        builder.Property(patient => patient.FullName).HasMaxLength(Patient.NameMaxLength);
        builder.Property(patient => patient.NationalId).HasMaxLength(Patient.NationalIdMaxLength);
        builder.Property(patient => patient.Phone).HasMaxLength(Patient.PhoneMaxLength);
        builder.Property(patient => patient.Gender).HasEnumAsString();
        builder.Property(patient => patient.FileNumber).HasDefaultValueSql($"NEXT VALUE FOR [{FileNumberSequence}]");

        builder.HasIndex(patient => patient.NationalId).IsUnique();
        builder.HasIndex(patient => patient.FileNumber).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Patient>(patient => patient.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
