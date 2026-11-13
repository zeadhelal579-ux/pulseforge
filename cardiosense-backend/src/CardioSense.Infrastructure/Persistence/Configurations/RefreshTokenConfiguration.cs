// =============================================================================
// File           : RefreshTokenConfiguration.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Maps RefreshToken to the RefreshTokens table.
// Logic          : Only the hash of a token is stored. The hash is unique because every refresh request finds its row
//                  by hash; lookups by account (revoke all sessions) use the foreign-key index.
// Implementation : IEntityTypeConfiguration picked up by ApplyConfigurationsFromAssembly.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping of <see cref="RefreshToken"/>.</summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ConfigureEntity("RefreshTokens");

        builder.Property(token => token.TokenHash).HasMaxLength(RefreshToken.TokenHashMaxLength);

        builder.HasIndex(token => token.TokenHash).IsUnique();

        builder.HasRestrictedForeignKey<RefreshToken, ApplicationUser>(token => token.UserId);
    }
}
