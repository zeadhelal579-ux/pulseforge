// =============================================================================
// File           : ApplicationUserConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps the login account to the Users table.
// Logic          : Identity already maps e-mail, password hash and lockout; this adds the table name from the
//                  database design plus the CardioSense columns (role as text, language, creation time).
// Implementation : IEntityTypeConfiguration picked up by ApplyConfigurationsFromAssembly.
// =============================================================================
using CardioSense.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="ApplicationUser"/>.</summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <summary>Longest preferred-language text ('ar' or 'en' today).</summary>
    private const int LanguageMaxLength = 10;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.Property(user => user.Role).HasEnumAsString();
        builder.Property(user => user.PreferredLanguage).HasMaxLength(LanguageMaxLength);
    }
}
