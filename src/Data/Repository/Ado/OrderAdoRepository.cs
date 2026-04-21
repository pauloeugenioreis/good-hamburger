using System.Data;
using System.Linq.Expressions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Data.Repository.Ado;

/// <summary>
/// ADO.NET implementation of Order repository
/// Demonstrates handling of complex entities with relationships using raw ADO.NET
/// Maximum control over transactions and database operations
/// </summary>
public class OrderAdoRepository : IOrderAdoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderAdoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Get order
        using var orderCommand = connection.CreateCommand();
        orderCommand.CommandText = @"
            SELECT Id, OrderNumber, CustomerName, CustomerEmail, CustomerPhone,
                   ShippingAddress, Status, Subtotal, Total, Notes,
                   IsActive, CreatedAt, UpdatedAt
            FROM Orders
            WHERE Id = @Id";

        AddParameter(orderCommand, "@Id", id);

        Order? order = null;
        using (var reader = await orderCommand.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                order = MapOrder(reader);
            }
        }

        if (order != null)
        {
            // Get order items
            using var itemsCommand = connection.CreateCommand();
            itemsCommand.CommandText = @"
                SELECT Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                       IsActive, CreatedAt, UpdatedAt
                FROM OrderItems
                WHERE OrderId = @OrderId";

            AddParameter(itemsCommand, "@OrderId", id);

            var items = new List<OrderItem>();
            using var itemsReader = await itemsCommand.ExecuteReaderAsync(cancellationToken);
            while (await itemsReader.ReadAsync(cancellationToken))
            {
                items.Add(MapOrderItem(itemsReader));
            }

            order.Items = items;
        }

        return order;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Get all orders
        var orders = new Dictionary<long, Order>();
        using var orderCommand = connection.CreateCommand();
        orderCommand.CommandText = @"
            SELECT Id, OrderNumber, CustomerName, CustomerEmail, CustomerPhone,
                   ShippingAddress, Status, Subtotal, Total, Notes,
                   IsActive, CreatedAt, UpdatedAt
            FROM Orders";

        using (var reader = await orderCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var order = MapOrder(reader);
                order.Items = new List<OrderItem>();
                orders[order.Id] = order;
            }
        }

        if (orders.Any())
        {
            // Get all order items
            using var itemsCommand = connection.CreateCommand();
            itemsCommand.CommandText = @"
                SELECT Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                       IsActive, CreatedAt, UpdatedAt
                FROM OrderItems";

            using var itemsReader = await itemsCommand.ExecuteReaderAsync(cancellationToken);
            while (await itemsReader.ReadAsync(cancellationToken))
            {
                var item = MapOrderItem(itemsReader);
                if (orders.TryGetValue(item.OrderId, out var order))
                {
                    order.Items.Add(item);
                }
            }
        }

        return orders.Values;
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
        await connection.OpenAsync(cancellationToken);

        // Begin transaction
        using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Insert order
            using var orderCommand = connection.CreateCommand();
            orderCommand.Transaction = transaction;
            orderCommand.CommandText = @"
                INSERT INTO Orders (OrderNumber, CustomerName, CustomerEmail, CustomerPhone,
                                  ShippingAddress, Status, Subtotal, Total, Notes,
                                  IsActive, CreatedAt, UpdatedAt)
                VALUES (@OrderNumber, @CustomerName, @CustomerEmail, @CustomerPhone,
                        @ShippingAddress, @Status, @Subtotal, @Total, @Notes,
                        @IsActive, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() as bigint)";

            entity.CreatedAt = DateTime.UtcNow;

            AddParameter(orderCommand, "@OrderNumber", entity.OrderNumber);
            AddParameter(orderCommand, "@CustomerName", entity.CustomerName);
            AddParameter(orderCommand, "@CustomerEmail", entity.CustomerEmail);
            AddParameter(orderCommand, "@CustomerPhone", entity.CustomerPhone ?? (object)DBNull.Value);
            AddParameter(orderCommand, "@ShippingAddress", entity.ShippingAddress);
            AddParameter(orderCommand, "@Status", entity.Status);
            AddParameter(orderCommand, "@Subtotal", entity.Subtotal);
            
            
            AddParameter(orderCommand, "@Total", entity.Total);
            AddParameter(orderCommand, "@Notes", entity.Notes ?? (object)DBNull.Value);
            AddParameter(orderCommand, "@IsActive", entity.IsActive);
            AddParameter(orderCommand, "@CreatedAt", entity.CreatedAt);
            AddParameter(orderCommand, "@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);

            var orderId = await orderCommand.ExecuteScalarAsync(cancellationToken);
            entity.Id = Convert.ToInt64(orderId, System.Globalization.CultureInfo.InvariantCulture);

            // Insert order items
            if (entity.Items?.Any() == true)
            {
                foreach (var item in entity.Items)
                {
                    using var itemCommand = connection.CreateCommand();
                    itemCommand.Transaction = transaction;
                    itemCommand.CommandText = @"
                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                                              IsActive, CreatedAt, UpdatedAt)
                        VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice, @Subtotal,
                                @IsActive, @CreatedAt, @UpdatedAt)";

                    item.OrderId = entity.Id;
                    item.CreatedAt = DateTime.UtcNow;

                    AddParameter(itemCommand, "@OrderId", item.OrderId);
                    AddParameter(itemCommand, "@ProductId", item.ProductId);
                    AddParameter(itemCommand, "@ProductName", item.ProductName);
                    AddParameter(itemCommand, "@Quantity", item.Quantity);
                    AddParameter(itemCommand, "@UnitPrice", item.UnitPrice);
                    AddParameter(itemCommand, "@Subtotal", item.Subtotal);
                    AddParameter(itemCommand, "@IsActive", item.IsActive);
                    AddParameter(itemCommand, "@CreatedAt", item.CreatedAt);
                    AddParameter(itemCommand, "@UpdatedAt", item.UpdatedAt ?? (object)DBNull.Value);

                    await itemCommand.ExecuteNonQueryAsync(cancellationToken);
                }
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
        await connection.OpenAsync(cancellationToken);

        using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Update order
            using var orderCommand = connection.CreateCommand();
            orderCommand.Transaction = transaction;
            orderCommand.CommandText = @"
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

            AddParameter(orderCommand, "@Id", entity.Id);
            AddParameter(orderCommand, "@OrderNumber", entity.OrderNumber);
            AddParameter(orderCommand, "@CustomerName", entity.CustomerName);
            AddParameter(orderCommand, "@CustomerEmail", entity.CustomerEmail);
            AddParameter(orderCommand, "@CustomerPhone", entity.CustomerPhone ?? (object)DBNull.Value);
            AddParameter(orderCommand, "@ShippingAddress", entity.ShippingAddress);
            AddParameter(orderCommand, "@Status", entity.Status);
            AddParameter(orderCommand, "@Subtotal", entity.Subtotal);
            
            
            AddParameter(orderCommand, "@Total", entity.Total);
            AddParameter(orderCommand, "@Notes", entity.Notes ?? (object)DBNull.Value);
            AddParameter(orderCommand, "@UpdatedAt", entity.UpdatedAt);

            await orderCommand.ExecuteNonQueryAsync(cancellationToken);

            // Delete existing items
            using var deleteCommand = connection.CreateCommand();
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
            AddParameter(deleteCommand, "@OrderId", entity.Id);
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            // Re-insert items
            if (entity.Items?.Count > 0)
            {
                foreach (var item in entity.Items)
                {
                    using var itemCommand = connection.CreateCommand();
                    itemCommand.Transaction = transaction;
                    itemCommand.CommandText = @"
                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                                              IsActive, CreatedAt, UpdatedAt)
                        VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice, @Subtotal,
                                @IsActive, @CreatedAt, @UpdatedAt)";

                    item.OrderId = entity.Id;
                    item.UpdatedAt = DateTime.UtcNow;

                    AddParameter(itemCommand, "@OrderId", item.OrderId);
                    AddParameter(itemCommand, "@ProductId", item.ProductId);
                    AddParameter(itemCommand, "@ProductName", item.ProductName);
                    AddParameter(itemCommand, "@Quantity", item.Quantity);
                    AddParameter(itemCommand, "@UnitPrice", item.UnitPrice);
                    AddParameter(itemCommand, "@Subtotal", item.Subtotal);
                    AddParameter(itemCommand, "@IsActive", item.IsActive);
                    AddParameter(itemCommand, "@CreatedAt", item.CreatedAt != default ? item.CreatedAt : DateTime.UtcNow);
                    AddParameter(itemCommand, "@UpdatedAt", item.UpdatedAt);

                    await itemCommand.ExecuteNonQueryAsync(cancellationToken);
                }
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
        await connection.OpenAsync(cancellationToken);

        using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Delete order items first
            using var deleteItemsCommand = connection.CreateCommand();
            deleteItemsCommand.Transaction = transaction;
            deleteItemsCommand.CommandText = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
            AddParameter(deleteItemsCommand, "@OrderId", entity.Id);
            await deleteItemsCommand.ExecuteNonQueryAsync(cancellationToken);

            // Delete order
            using var deleteOrderCommand = connection.CreateCommand();
            deleteOrderCommand.Transaction = transaction;
            deleteOrderCommand.CommandText = "DELETE FROM Orders WHERE Id = @Id";
            AddParameter(deleteOrderCommand, "@Id", entity.Id);
            await deleteOrderCommand.ExecuteNonQueryAsync(cancellationToken);

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
        await connection.OpenAsync(cancellationToken);

        // Get total count
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Orders";
        var total = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);

        // Get paged orders
        var orders = new Dictionary<long, Order>();
        using var orderCommand = connection.CreateCommand();
        orderCommand.CommandText = @"
            SELECT Id, OrderNumber, CustomerName, CustomerEmail, CustomerPhone,
                   ShippingAddress, Status, Subtotal, Total, Notes,
                   IsActive, CreatedAt, UpdatedAt
            FROM Orders
            ORDER BY Id
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        AddParameter(orderCommand, "@Offset", (page - 1) * pageSize);
        AddParameter(orderCommand, "@PageSize", pageSize);

        using (var reader = await orderCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var order = MapOrder(reader);
                order.Items = new List<OrderItem>();
                orders[order.Id] = order;
            }
        }

        if (orders.Any())
        {
            // Get items for these orders
            var orderIds = string.Join(",", orders.Keys);
            using var itemsCommand = connection.CreateCommand();
            itemsCommand.CommandText = $@"
                SELECT Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal,
                       IsActive, CreatedAt, UpdatedAt
                FROM OrderItems
                WHERE OrderId IN ({orderIds})";

            using var itemsReader = await itemsCommand.ExecuteReaderAsync(cancellationToken);
            while (await itemsReader.ReadAsync(cancellationToken))
            {
                var item = MapOrderItem(itemsReader);
                if (orders.TryGetValue(item.OrderId, out var order))
                {
                    order.Items.Add(item);
                }
            }
        }

        return (orders.Values, total);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ADO.NET doesn't have a unit of work pattern
        // Changes are immediately persisted
        return Task.FromResult(0);
    }

    #region Helper Methods

    private static Order MapOrder(IDataReader reader)
    {
        return new Order
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
            CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
            CustomerPhone = reader.IsDBNull(reader.GetOrdinal("CustomerPhone")) ? null : reader.GetString(reader.GetOrdinal("CustomerPhone")),
            ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
            
            
            Total = reader.GetDecimal(reader.GetOrdinal("Total")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }

    private static OrderItem MapOrderItem(IDataReader reader)
    {
        return new OrderItem
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
            ProductId = reader.GetInt64(reader.GetOrdinal("ProductId")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
            Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
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
