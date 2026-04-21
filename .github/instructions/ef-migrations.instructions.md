---
description: "Use when creating EF Core migrations, modifying database schema, adding entity mappings, or working with DbContext. Covers migration safety, rollback patterns, and multi-database support."
---

# EF Core Migration Guidelines

## Multi-Database Support
This project supports SQL Server, PostgreSQL, MySQL, and Oracle. Migrations must be compatible with all providers.

## Creating Migrations
```bash
dotnet ef migrations add <MigrationName> --project src/Data --startup-project src/Api
```

## Rules
- Always create reversible migrations with proper `Down()` methods
- Never drop columns in the same release as code removal
- Test migrations against all configured database providers
- Use `IEntityTypeConfiguration<T>` in `Data/Mappings/` for entity configuration
- Never use data annotations on entities — use Fluent API exclusively
- Run `dotnet ef migrations script` to review generated SQL before applying

## Entity Configuration Pattern
```csharp
public class ProductMapping : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
    }
}
```

## Context
- Main context: `ApplicationDbContext` in `Data/Context/`
- Factory for migrations: `ApplicationDbContextFactory`
- Database provider is configured via `AppSettings.Infrastructure.Database.DatabaseType`
