using FluentNHibernate.Mapping;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Data.Mappings.NHibernate;

/// <summary>
/// NHibernate mapping for Product entity
/// </summary>
public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Table("Products");

        Id(x => x.Id)
            .GeneratedBy.Identity()
            .Column("Id");

        Map(x => x.Name)
            .Not.Nullable()
            .Length(200);

        Map(x => x.Description)
            .Nullable()
            .Length(1000);

        Map(x => x.Price)
            .Not.Nullable()
            .Precision(18)
            .Scale(2);

        Map(x => x.Category)
            .Not.Nullable()
            .Length(100);

        Map(x => x.IsActive)
            .Not.Nullable();

        Map(x => x.CreatedAt)
            .Not.Nullable();

        Map(x => x.UpdatedAt)
            .Nullable();
    }
}
