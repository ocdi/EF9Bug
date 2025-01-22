// See https://aka.ms/new-console-template for more information
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace EF9Bug;

public class Item
{
    public int Id { get; set; }
    public string? OwnerId { get; set; }
    public string? OwnerType { get; set; }

    public required string Name { get; set; }
    public required DoubleNested DoubleNested { get; set; }
    public OptionalOne? Optional { get; set; }
    public OptionalTwo? Optional2 { get; set; }

    public ICollection<StrugglingToFindIt> It { get; set; }
    public ICollection<Detail> Details { get; set; }
}

public class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
{
    public required DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Item>()
            .HasMany(d => d.It)
            .WithMany(d => d.Items)
            .UsingEntity<Detail>(a => a.ToTable("Detail"));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Only applicable for entities implementing ISoftDeletableEntity
            if (!typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            // 1. Add the IsDeleted property
            //https://github.com/dotnet/efcore/issues/19310
            var isDelProp =
                entityType.FindProperty(nameof(ISoftDeletableEntity.IsDeleted))
                ?? entityType.AddProperty(nameof(ISoftDeletableEntity.IsDeleted), typeof(bool));

            // 2. Create the query filter
            var parameter = Expression.Parameter(entityType.ClrType);

            // EF.Property<bool>(post, "IsDeleted")
            var propertyMethodInfo = typeof(EF)
                .GetMethod("Property")
                .MakeGenericMethod(typeof(bool));
            var isDeletedProperty = Expression.Call(
                propertyMethodInfo,
                parameter,
                Expression.Constant(nameof(ISoftDeletableEntity.IsDeleted))
            );

            // EF.Property<bool>(post, "IsDeleted") == false
            BinaryExpression compareExpression = Expression.MakeBinary(
                ExpressionType.Equal,
                isDeletedProperty,
                Expression.Constant(false)
            );

            // post => EF.Property<bool>(post, "IsDeleted") == false
            var lambda = Expression.Lambda(compareExpression, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        //modelBuilder.Entity<OptionalOne>().HasQueryFilter(a => !a.Deleted);
    }
}

internal interface ISoftDeletableEntity
{
    bool IsDeleted { get; }
}

public class DoubleNested
{
    public int Id { get; set; }
    public required Nested Nested { get; set; }
}

public class Nested
{
    public int Id { get; set; }
    public Category? Category { get; set; }
}

public class Detail
{
    public int Id { get; set; }

    public StrugglingToFindIt It { get; set; }
}

public class StrugglingToFindIt
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int Count { get; set; }

    public ICollection<Item> Items { get; set; } = [];
}

public class OptionalOne : ISoftDeletableEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
}

public class OptionalTwo : ISoftDeletableEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}
