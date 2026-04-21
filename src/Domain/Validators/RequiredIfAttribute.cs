using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Domain.Validators;

/// <summary>
/// Validates that a property is required when a condition is met
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _propertyValue;

    public string PropertyName => _propertyName;
    public object PropertyValue => _propertyValue;

    public RequiredIfAttribute(string propertyName, object propertyValue)
    {
        _propertyName = propertyName;
        _propertyValue = propertyValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var propertyInfo = instance.GetType().GetProperty(_propertyName);

        if (propertyInfo == null)
        {
            return new ValidationResult($"Propriedade {_propertyName} não encontrada");
        }

        var propertyValue = propertyInfo.GetValue(instance);

        // Se a condição for verdadeira, o campo é obrigatório
        if (Equals(propertyValue, _propertyValue) && (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
        {
            return new ValidationResult(
                ErrorMessage ?? $"{validationContext.DisplayName} é obrigatório quando {_propertyName} é igual a {_propertyValue}");
        }

        return ValidationResult.Success;
    }
}
