using System.Data.Common;
using Microsoft.Data.SqlClient; // Updated from System.Data.SqlClient (obsolete)
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Infrastructure.Services;

/// <summary>
/// SQL Server connection factory implementation
/// Registered in DI and injected into Dapper and ADO.NET repositories
/// Returns DbConnection to support async operations
/// Note: Uses Microsoft.Data.SqlClient (modern) instead of System.Data.SqlClient (obsolete)
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
