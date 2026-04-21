using System.Collections.Generic;
using System.Linq;

namespace GoodHamburger.Domain.Entities;

/// <summary>
/// Canonical list of order statuses plus helpers to validate/normalize input
/// </summary>
public static class OrderStatus
{
    public const string Pending = "Pendente";
    public const string Processing = "Em Processamento";
    public const string Shipped = "Enviado";
    public const string Delivered = "Entregue";
    public const string Cancelled = "Cancelado";

    private static readonly string[] _allowedStatuses =
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    };

    private static readonly IReadOnlyDictionary<string, string> _lookup = BuildLookup();

    /// <summary>
    /// Gets the full set of supported statuses in their canonical format.
    /// </summary>
    public static IReadOnlyCollection<string> AllowedStatuses => _allowedStatuses;

    /// <summary>
    /// Attempts to normalize any casing/spacing of the incoming status.
    /// </summary>
    public static bool TryNormalize(string? status, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var key = status.Trim().ToUpperInvariant();
        if (_lookup.TryGetValue(key, out var canonical))
        {
            normalized = canonical;
            return true;
        }

        return false;
    }

    private static IReadOnlyDictionary<string, string> BuildLookup()
    {
        var lookup = _allowedStatuses.ToDictionary(
            status => status.ToUpperInvariant(),
            status => status);

        // Backward-compatible aliases accepted by older clients/tests.
        lookup["PENDING"] = Pending;
        lookup["PROCESSING"] = Processing;
        lookup["SHIPPED"] = Shipped;
        lookup["DELIVERED"] = Delivered;
        lookup["CANCELLED"] = Cancelled;
        lookup["CANCELED"] = Cancelled;

        return lookup;
    }
}
