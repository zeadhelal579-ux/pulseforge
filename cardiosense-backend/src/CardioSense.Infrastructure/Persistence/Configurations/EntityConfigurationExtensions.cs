// =============================================================================
// File           : EntityConfigurationExtensions.cs
// Layer          : Infrastructure / Persistence / Configurations
// Purpose        : Shared EF Core mapping helpers so the thirteen entity configurations stay short and consistent.
// Logic          : Every entity repeats the same three decisions - table + client-generated key, enums stored as
//                  readable text, and foreign keys that never cascade-delete (medical data must not vanish silently;
//                  removal is a deliberate soft-delete/anonymisation flow). Each decision lives here once.
// Implementation : Internal static extension methods over EntityTypeBuilder / PropertyBuilder.
// =============================================================================
using System.Linq.Expressions;
using CardioSense.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardioSense.Infrastructure.Persistence.Configurations;

/// <summary>Mapping helpers shared by the entity configurations.</summary>
internal static class EntityConfigurationExtensions
{
    /// <summary>Column length used for every enum stored as text (longest value is 16 characters).</summary>
    private const int EnumMaxLength = 32;

    /// <summary>Maps the entity to a table and declares its key (ids come from the entity, not the database).</summary>
    public static EntityTypeBuilder<TEntity> ConfigureEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder, string tableName)
        where TEntity : Entity
    {
        builder.ToTable(tableName);
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        return builder;
    }

    /// <summary>Stores an enum as its name (readable in SQL and safe if the member order changes).</summary>
    public static PropertyBuilder<TEnum> HasEnumAsString<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum =>
        property.HasConversion<string>().HasMaxLength(EnumMaxLength);

    /// <summary>Adds a required or optional foreign key to another entity with delete restricted.</summary>
    public static void HasRestrictedForeignKey<TEntity, TPrincipal>(
        this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, object?>> foreignKey)
        where TEntity : class
        where TPrincipal : class =>
        builder.HasOne<TPrincipal>().WithMany().HasForeignKey(foreignKey).OnDelete(DeleteBehavior.Restrict);
}
