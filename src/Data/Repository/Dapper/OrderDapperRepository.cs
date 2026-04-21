using System.Data;
using System.Linq.Expressions;
using Dapper;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.Dapper;

/// <summary>
/// Dapper implementation of Order repository
/// Demonstrates handling of complex entities with relationships
/// Uses IDbConnectionFactory injected via DI for proper connection management
/// </summary>
public class OrderDapperRepository : IOrderDapperRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderDapperRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Get order
        const string orderSql = @"
            SELECT Id, OrderNumber, CustomerName, CustomerEmail, CustomerPhone,
                   ShippingAddress, Status, Subtotal, Total, Notes,
                   IsActive, CreatedAt, UpdatedAt
            FROM Orders
            WHERE Id = @Id";

        var order = await connection.QuerySingleOrDefaultAsync<Order>(orderSql, new { Id = id });

        if (order != null)
        {
            // Get order items
            const string itemsSql = @"
                SELECT Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                       IsActive, CreatedAt, UpdatedAt
                FROM OrderItems
                WHERE OrderId = @OrderId";

            var items = await connection.QueryAsync<OrderItem>(itemsSql, new { OrderId = id });
            order.Items = items.ToList();
        }

        return order;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT o.Id, o.OrderNumber, o.CustomerName, o.CustomerEmail, o.CustomerPhone,
                   o.ShippingAddress, o.Status, o.Subtotal, o.Total, o.Notes,
                   o.IsActive, o.CreatedAt, o.UpdatedAt,
                   oi.Id, oi.OrderId, oi.ProductId, oi.ProductName, oi.Quantity, oi.UnitPrice, oi.Subtotal,
                   oi.IsActive, oi.CreatedAt, oi.UpdatedAt
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId";

        var orderDict = new Dictionary<long, Order>();

        await connection.QueryAsync<Order, OrderItem, Order>(
            sql,
            (order, orderItem) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var orderEntry))
                {
                    orderEntry = order;
                    orderEntry.Items = new List<OrderItem>();
                    orderDict.Add(orderEntry.Id, orderEntry);
                }

                if (orderItem != null)
                {
                    orderEntry.Items.Add(orderItem);
                }

                return orderEntry;
            },
            splitOn: "Id");

        return orderDict.Values;
    }

    public async Task<IEnumerable<Order>> FindAsync(
        Expression<Func<Order, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var compiledPredicate = predicate.Compile();
        var all = await GetAllAsync(cancellationToken);
        return all.Where(compiledPredicate);
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Insert order
            const string orderSql = @"
                INSERT INTO Orders (OrderNumber, CustomerName, CustomerEmail, CustomerPhone,
                                  ShippingAddress, Status, Subtotal, Total, Notes,
                                  IsActive, CreatedAt, UpdatedAt)
                VALUES (@OrderNumber, @CustomerName, @CustomerEmail, @CustomerPhone,
                        @ShippingAddress, @Status, @Subtotal, @Total, @Notes,
                        @IsActive, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint)";

            entity.CreatedAt = DateTime.UtcNow;
            var orderId = await connection.QuerySingleAsync<long>(orderSql, entity, transaction);
            entity.Id = orderId;

            // Insert order items
            if (entity.Items?.Count > 0)
            {
                const string itemSql = @"
                    INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                                          IsActive, CreatedAt, UpdatedAt)
                    VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice, @Subtotal,
                            @IsActive, @CreatedAt, @UpdatedAt)";

                foreach (var item in entity.Items)
                {
                    item.OrderId = orderId;
                    item.CreatedAt = DateTime.UtcNow;
                }

                await connection.ExecuteAsync(itemSql, entity.Items, transaction);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return entity;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            await AddAsync(entity, cancellationToken);
        }
        return entityList;
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Update order
            const string orderSql = @"
                UPDATE Orders
                SET OrderNumber = @OrderNumber,
                    CustomerName = @CustomerName,
                    CustomerEmail = @CustomerEmail,
                    CustomerPhone = @CustomerPhone,
                    ShippingAddress = @ShippingAddress,
                    Status = @Status,
                    Subtotal = @Subtotal,
                    
                    
                    Total = @Total,
                    Notes = @Notes,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            entity.UpdatedAt = DateTime.UtcNow;
            await connection.ExecuteAsync(orderSql, entity, transaction);

            // Delete existing items and re-insert
            const string deleteItemsSql = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
            await connection.ExecuteAsync(deleteItemsSql, new { OrderId = entity.Id }, transaction);

            if (entity.Items?.Count > 0)
            {
                const string itemSql = @"
                    INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                                          IsActive, CreatedAt, UpdatedAt)
                    VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice, @Subtotal,
                            @IsActive, @CreatedAt, @UpdatedAt)";

                foreach (var item in entity.Items)
                {
                    item.OrderId = entity.Id;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await connection.ExecuteAsync(itemSql, entity.Items, transaction);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Delete order items first (foreign key constraint)
            const string deleteItemsSql = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
            await connection.ExecuteAsync(deleteItemsSql, new { OrderId = entity.Id }, transaction);

            // Delete order
            const string deleteOrderSql = "DELETE FROM Orders WHERE Id = @Id";
            await connection.ExecuteAsync(deleteOrderSql, new { entity.Id }, transaction);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string countSql = "SELECT COUNT(*) FROM Orders";
        var total = await connection.ExecuteScalarAsync<int>(countSql);

        const string dataSql = @"
            SELECT o.Id, o.OrderNumber, o.CustomerName, o.CustomerEmail, o.CustomerPhone,
                   o.ShippingAddress, o.Status, o.Subtotal, o.Total, o.Notes,
                   o.IsActive, o.CreatedAt, o.UpdatedAt,
                   oi.Id, oi.OrderId, oi.ProductId, oi.ProductName, oi.Quantity, oi.UnitPrice, oi.Subtotal,
                   oi.IsActive, oi.CreatedAt, oi.UpdatedAt
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
            ORDER BY o.Id
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var orderDict = new Dictionary<long, Order>();

        await connection.QueryAsync<Order, OrderItem, Order>(
            dataSql,
            (order, orderItem) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var orderEntry))
                {
                    orderEntry = order;
                    orderEntry.Items = new List<OrderItem>();
                    orderDict.Add(orderEntry.Id, orderEntry);
                }

                if (orderItem != null)
                {
                    orderEntry.Items.Add(orderItem);
                }

                return orderEntry;
            },
            new { Offset = (page - 1) * pageSize, PageSize = pageSize },
            splitOn: "Id");

        return (orderDict.Values, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dapper doesn't have a unit of work pattern by default
        // Changes are immediately persisted
        return Task.FromResult(0);
    }
}
