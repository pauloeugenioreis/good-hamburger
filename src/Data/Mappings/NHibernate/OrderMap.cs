using FluentNHibernate.Mapping;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Data.Mappings.NHibernate;

/// <summary>
/// NHibernate mapping for Order entity
/// </summary>
public class OrderMap : ClassMap<Order>
{
    public OrderMap()
    {
        Table("Orders");
        
        Id(x => x.Id)
            .GeneratedBy.Identity()
            .Column("Id");
        
        Map(x => x.OrderNumber)
            .Not.Nullable()
            .Length(50)
            .UniqueKey("UK_Orders_OrderNumber");
        
        Map(x => x.CustomerName)
            .Not.Nullable()
            .Length(200);
        
        Map(x => x.CustomerEmail)
            .Not.Nullable()
            .Length(200);
        
        Map(x => x.CustomerPhone)
            .Nullable()
            .Length(20);
        
        Map(x => x.ShippingAddress)
            .Not.Nullable()
            .Length(500);
        
        Map(x => x.Status)
            .Not.Nullable()
            .Length(50);
        
        Map(x => x.Subtotal)
            .Not.Nullable()
            .Precision(18)
            .Scale(2);
        
        Map(x => x.Total)
            .Not.Nullable()
            .Precision(18)
            .Scale(2);
        
        Map(x => x.Notes)
            .Nullable()
            .Length(1000);
        
        Map(x => x.IsActive)
            .Not.Nullable();
        
        Map(x => x.CreatedAt)
            .Not.Nullable();
        
        Map(x => x.UpdatedAt)
            .Nullable();
        
        HasMany(x => x.Items)
            .KeyColumn("OrderId")
            .Cascade.AllDeleteOrphan()
            .Inverse();
    }
}

/// <summary>
/// NHibernate mapping for OrderItem entity
/// </summary>
public class OrderItemMap : ClassMap<OrderItem>
{
    public OrderItemMap()
    {
        Table("OrderItems");
        
        Id(x => x.Id)
            .GeneratedBy.Identity()
            .Column("Id");
        
        Map(x => x.OrderId)
            .Not.Nullable();
        
        Map(x => x.ProductId)
            .Not.Nullable();
        
        Map(x => x.ProductName)
            .Not.Nullable()
            .Length(200);
        
        Map(x => x.Quantity)
            .Not.Nullable();
        
        Map(x => x.UnitPrice)
            .Not.Nullable()
            .Precision(18)
            .Scale(2);
        
        Map(x => x.Subtotal)
            .Not.Nullable()
            .Precision(18)
            .Scale(2);
        
        Map(x => x.IsActive)
            .Not.Nullable();
        
        Map(x => x.CreatedAt)
            .Not.Nullable();
        
        Map(x => x.UpdatedAt)
            .Nullable();
        
        References(x => x.Order)
            .Column("OrderId")
            .Not.Insert()
            .Not.Update();
        
        References(x => x.Product)
            .Column("ProductId")
            .LazyLoad();
    }
}
