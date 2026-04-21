using Microsoft.EntityFrameworkCore;
using GoodHamburger.Data.Context;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Domain.Events;

namespace GoodHamburger.Data.Seeders;

public sealed class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IEventStore _eventStore;

    public DbSeeder(ApplicationDbContext context, IEventStore eventStore)
    {
        _context = context;
        _eventStore = eventStore;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var products = new List<Product>
        {
            new()
            {
                Name = "X Burger",
                Description = "Sanduíche",
                Price = 5.00m,
                Category = "Sanduíches",
                IsActive = true
            },
            new()
            {
                Name = "X Egg",
                Description = "Sanduíche",
                Price = 4.50m,
                Category = "Sanduíches",
                IsActive = true
            },
            new()
            {
                Name = "X Bacon",
                Description = "Sanduíche",
                Price = 7.00m,
                Category = "Sanduíches",
                IsActive = true
            },
            new()
            {
                Name = "Batata frita",
                Description = "Acompanhamento",
                Price = 2.00m,
                Category = "Batatas",
                IsActive = true
            },
            new()
            {
                Name = "Refrigerante",
                Description = "Acompanhamento",
                Price = 2.50m,
                Category = "Refrigerantes",
                IsActive = true
            }
        };

        await _context.Products.AddRangeAsync(products, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Append events for the newly inserted products
        foreach (var product in products)
        {
            var createdEvent = new ProductCreatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                IsActive = product.IsActive
            };
            await _eventStore.AppendEventAsync("Product", product.Id.ToString(), createdEvent, cancellationToken: cancellationToken);
        }
    }
}
