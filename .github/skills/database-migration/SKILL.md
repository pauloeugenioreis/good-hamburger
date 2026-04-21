---
name: database-migration
description: "Create and manage EF Core database migrations safely. Use when: adding a migration, modifying schema, changing entity properties, adding indexes, updating database structure."
argument-hint: "Migration description (e.g., AddCustomerTable)"
---

# Database Migration

Safely create and manage EF Core migrations with multi-database awareness.

## When to Use
- Adding new tables or entities
- Modifying entity properties (columns)
- Adding indexes or constraints
- Changing relationships

## Procedure

### 1. Pre-flight Checks
1. Verify the entity/mapping changes are complete in `src/Data/Mappings/`
2. Ensure `DbSet<T>` is registered in `ApplicationDbContext`
3. Check that no pending migrations exist: `dotnet ef migrations list --project src/Data --startup-project src/Api`

### 2. Create Migration
4. Generate migration:
```bash
dotnet ef migrations add {MigrationName} --project src/Data --startup-project src/Api
```

### 3. Review Migration
5. Open the generated migration in `src/Data/Migrations/`
6. Verify the `Up()` method creates the expected schema
7. Verify the `Down()` method properly reverses all changes
8. Generate SQL script for review:
```bash
dotnet ef migrations script --project src/Data --startup-project src/Api
```

### 4. Validate
9. Apply migration to development database:
```bash
dotnet ef database update --project src/Data --startup-project src/Api
```
10. Test rollback:
```bash
dotnet ef database update {PreviousMigration} --project src/Data --startup-project src/Api
```

## Safety Rules
- Never use `migrationBuilder.Sql()` with user input
- Always provide reversible `Down()` methods
- Never drop columns and remove code in the same release
- Test migration against all supported database providers
- Use `HasMaxLength()` on all string properties
