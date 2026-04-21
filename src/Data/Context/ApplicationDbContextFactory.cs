using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoodHamburger.Data.Context;

/// <summary>
/// Design-time factory for creating ApplicationDbContext instances
/// Used by EF Core tools for migrations
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Create a minimal options builder just for design-time
        // The actual configuration is in appsettings.json and Program.cs
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use SQL Server with a placeholder connection string for design-time
        // This allows migrations to be generated without a real database
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GoodHamburger;Trusted_Connection=True;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
