using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Data.Context;

/// <summary>
/// Linq2Db DataConnection - High-performance LINQ provider
/// </summary>
public class ApplicationDataConnection : DataConnection
{
    public ApplicationDataConnection(LinqToDbConnectionOptions<ApplicationDataConnection> options)
        : base(options)
    {
    }

    // Tables
    public ITable<Product> Products => this.GetTable<Product>();
    public ITable<Order> Orders => this.GetTable<Order>();
    public ITable<OrderItem> OrderItems => this.GetTable<OrderItem>();
}
