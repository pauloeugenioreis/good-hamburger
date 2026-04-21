using System.Data;
using System.Linq.Expressions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.Ado;

/// <summary>
/// ADO.NET implementation of Product repository
/// Maximum performance with full control over database operations
/// Uses raw ADO.NET with SqlCommand and SqlDataReader
/// </summary>
public class ProductAdoRepository : IProductAdoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductAdoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Price, Category, IsActive, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id = @Id";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapProduct(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Price, Category, IsActive, CreatedAt, UpdatedAt
            FROM Products";

        var products = new List<Product>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<IEnumerable<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var compiledPredicate = predicate.Compile();
        var all = await GetAllAsync(cancellationToken);
        return all.Where(compiledPredicate);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Products (Name, Description, Price, Category, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Description, @Price, @Category, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT CAST(SCOPE_IDENTITY() as bigint)";

        entity.CreatedAt = DateTime.UtcNow;

        AddParameter(command, "@Name", entity.Name);
        AddParameter(command, "@Description", entity.Description ?? (object)DBNull.Value);
        AddParameter(command, "@Price", entity.Price);
        AddParameter(command, "@Category", entity.Category);
        AddParameter(command, "@IsActive", entity.IsActive);
        AddParameter(command, "@CreatedAt", entity.CreatedAt);
        AddParameter(command, "@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);

        var id = await command.ExecuteScalarAsync(cancellationToken);
        entity.Id = Convert.ToInt64(id, System.Globalization.CultureInfo.InvariantCulture);

        return entity;
    }

    public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            await AddAsync(entity, cancellationToken);
        }
        return entityList;
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                Category = @Category,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        entity.UpdatedAt = DateTime.UtcNow;

        AddParameter(command, "@Id", entity.Id);
        AddParameter(command, "@Name", entity.Name);
        AddParameter(command, "@Description", entity.Description ?? (object)DBNull.Value);
        AddParameter(command, "@Price", entity.Price);
        AddParameter(command, "@Category", entity.Category);
        AddParameter(command, "@IsActive", entity.IsActive);
        AddParameter(command, "@UpdatedAt", entity.UpdatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = @Id";

        AddParameter(command, "@Id", entity.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Products";
        var total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);

        // Get paged data
        using var dataCommand = connection.CreateCommand();
        dataCommand.CommandText = @"
            SELECT Id, Name, Description, Price, Category, IsActive, CreatedAt, UpdatedAt
            FROM Products
            ORDER BY Id
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        AddParameter(dataCommand, "@Offset", (page - 1) * pageSize);
        AddParameter(dataCommand, "@PageSize", pageSize);

        var products = new List<Product>();
        using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return (products, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ADO.NET doesn't have a unit of work pattern
        // Changes are immediately persisted
        return Task.FromResult(0);
    }

    #region Helper Methods

    private static Product MapProduct(IDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            Category = reader.GetString(reader.GetOrdinal("Category")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    #endregion
}
