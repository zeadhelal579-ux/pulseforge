// =============================================================================
// File           : DeviceConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps Device to the Devices table.
// Logic          : The serial number is unique. PatientId is nullable (inventory devices belong to nobody) and, among
//                  paired devices, unique - the database enforces "one device per patient" even if two pairing
//                  requests race past the handler's check.
// Implementation : Filtered unique index (SQL Server) so many unpaired rows can coexist.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="Device"/>.</summary>
public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ConfigureEntity("Devices");

        builder.Property(device => device.SerialNumber).HasMaxLength(Device.SerialNumberMaxLength);
        builder.Property(device => device.FirmwareVersion).HasMaxLength(Device.FirmwareVersionMaxLength);
        builder.Property(device => device.ConnectionMode).HasEnumAsString();

        builder.HasIndex(device => device.SerialNumber).IsUnique();
        builder.HasIndex(device => device.PatientId).IsUnique().HasFilter("[PatientId] IS NOT NULL");

        builder.HasRestrictedForeignKey<Device, Patient>(device => device.PatientId);
    }
}
