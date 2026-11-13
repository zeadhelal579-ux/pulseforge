// =============================================================================
// File           : DoctorConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps Doctor to the Doctors table.
// Logic          : The license number is unique (one profile per real doctor) and each login account owns at most one
//                  doctor profile.
// Implementation : IEntityTypeConfiguration picked up by ApplyConfigurationsFromAssembly.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="Doctor"/>.</summary>
public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ConfigureEntity("Doctors");

        builder.Property(doctor => doctor.FullName).HasMaxLength(Doctor.NameMaxLength);
        builder.Property(doctor => doctor.Specialty).HasMaxLength(Doctor.SpecialtyMaxLength);
        builder.Property(doctor => doctor.LicenseNumber).HasMaxLength(Doctor.LicenseNumberMaxLength);
        builder.Property(doctor => doctor.Hospital).HasMaxLength(Doctor.HospitalMaxLength);
        builder.Property(doctor => doctor.Department).HasMaxLength(Doctor.DepartmentMaxLength);
        builder.Property(doctor => doctor.Phone).HasMaxLength(Doctor.PhoneMaxLength);

        builder.HasIndex(doctor => doctor.LicenseNumber).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Doctor>(doctor => doctor.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
