using System.Data;
using System.Data.Common;

namespace GoodHamburger.Domain.Interfaces;

/// <summary>
/// Factory for creating database connections
/// Used by Dapper and ADO.NET repositories to get connections from DI
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection
    /// Returns DbConnection (not IDbConnection) to support async operations
    /// </summary>
    /// <returns>Database connection ready to use</returns>
    DbConnection CreateConnection();
}
