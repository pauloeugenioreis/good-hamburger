using FluentValidation;
using GoodHamburger.Domain.Dtos;

namespace GoodHamburger.Domain.Validators;

file static class ProductCategoryRules
{
    public static readonly HashSet<string> AllowedCategories =
    [
        "Sanduíches",
        "Batatas",
        "Refrigerantes"
    ];
}

/// <summary>
/// Validation rules for creating products.
/// </summary>
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("O nome do produto não pode exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("A descrição não pode exceder 2000 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("A categoria é obrigatória")
            .MaximumLength(120).WithMessage("A categoria não pode exceder 120 caracteres")
            .Must(category => ProductCategoryRules.AllowedCategories.Contains(category.Trim()))
            .WithMessage("A categoria deve ser uma das seguintes: Sanduíches, Batatas, Refrigerantes");
    }
}

/// <summary>
/// Validation rules for updating products.
/// </summary>
public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("O nome do produto não pode exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("A descrição não pode exceder 2000 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("A categoria é obrigatória")
            .MaximumLength(120).WithMessage("A categoria não pode exceder 120 caracteres")
            .Must(category => ProductCategoryRules.AllowedCategories.Contains(category.Trim()))
            .WithMessage("A categoria deve ser uma das seguintes: Sanduíches, Batatas, Refrigerantes");
    }
}

/// <summary>
/// Validation rules for toggling product status.
/// </summary>
public class UpdateProductStatusValidator : AbstractValidator<UpdateProductStatusRequest>
{
    public UpdateProductStatusValidator()
    {
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("O campo IsActive deve ser informado");
    }
}
