using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Domain.Validators;

/// <summary>
/// Custom validation attribute for Redis connection strings
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class RedisConnectionStringAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string connectionString)
        {
            return ValidationResult.Success;
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new ValidationResult("A string de conexão do Redis não pode estar vazia");
        }

        // Validar formato básico: host:port ou host:port,ssl=true
        if (!connectionString.Contains(':'))
        {
            return new ValidationResult(
                "A string de conexão do Redis deve conter host e porta (ex: 'localhost:6379')");
        }

        return ValidationResult.Success;
    }
}
