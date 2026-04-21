using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GoodHamburger.Domain;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Services;
using Weasel.Core;

namespace GoodHamburger.Infrastructure.Extensions;

/// <summary>
/// Event Sourcing infrastructure configuration
/// </summary>
public static class EventSourcingExtension
{
    /// <summary>
    /// Add Event Sourcing services to the DI container
    /// </summary>
    public static IServiceCollection AddEventSourcing(this IServiceCollection services, IOptions<AppSettings> appSettings)
    {
        var settings = appSettings.Value.Infrastructure.EventSourcing;

        // Register settings so repositories/controllers can resolve even when disabled
        services.AddSingleton(settings);

        if (!settings.Enabled)
        {
            // Event Sourcing disabled - register empty implementation
            services.AddScoped<IEventStore, NoOpEventStore>();
            return services;
        }

        // Configure based on provider
        switch (settings.Provider.ToLowerInvariant())
        {
            case "marten":
                ConfigureMarten(services, settings);
                break;

            case "custom":
                // For future custom implementations
                services.AddScoped<IEventStore, NoOpEventStore>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported Event Sourcing provider: {settings.Provider}");
        }

        return services;
    }

    private static void ConfigureMarten(IServiceCollection services, EventSourcingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "EventSourcing:ConnectionString is required when using Marten provider");
        }

        // Configure Marten with simplified settings for Marten 8.x
        services.AddMarten(options =>
        {
            options.Connection(settings.ConnectionString);

            var streamIdentityProperty = options.Events.GetType().GetProperty(nameof(options.Events.StreamIdentity));
            if (streamIdentityProperty?.PropertyType.IsEnum == true)
            {
                var streamIdentityValue = Enum.Parse(streamIdentityProperty.PropertyType, "AsString");
                streamIdentityProperty.SetValue(options.Events, streamIdentityValue);
            }
        });

        // Register Event Store implementation
        services.AddScoped<IEventStore, MartenEventStore>();
    }
}

/// <summary>
/// No-op Event Store implementation when Event Sourcing is disabled
/// </summary>
internal class NoOpEventStore : IEventStore
{
    public Task AppendEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent eventData,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        // No-op
        return Task.CompletedTask;
    }

    public Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<DomainEvent>());
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsPagedAsync(
        string aggregateType,
        string aggregateId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult((new List<DomainEvent>(), 0L));
    }

    public Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        DateTime until,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<DomainEvent>());
    }

    public Task<List<DomainEvent>> GetEventsByVersionAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<DomainEvent>());
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByTypeAsync(
        string aggregateType,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult((new List<DomainEvent>(), 0L));
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByUserAsync(
        string userId,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult((new List<DomainEvent>(), 0L));
    }

    public Task<int> GetLatestVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task SaveSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        TSnapshot snapshot,
        int version,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        return Task.CompletedTask;
    }

    public Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        return Task.FromResult<(TSnapshot?, int)>((null, 0));
    }

    public Task<EventStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new EventStatistics());
    }
}
