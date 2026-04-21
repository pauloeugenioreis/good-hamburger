using FluentValidation;
using GoodHamburger.Domain.Dtos;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Validators;

/// <summary>
/// Validator for CreateOrderRequest
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório")
            .MaximumLength(200).WithMessage("O nome do cliente não pode exceder 200 caracteres");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("O e-mail é obrigatório")
            .EmailAddress().WithMessage("Formato de e-mail inválido");
        /*
        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("O endereço de entrega é obrigatório")
            .MaximumLength(500).WithMessage("O endereço de entrega não pode exceder 500 caracteres");
        */

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos um item")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("O pedido deve conter pelo menos um item")
            .Must(items => items == null || items.Count <= 3)
            .WithMessage("O pedido pode conter no máximo 3 itens: um sanduíche, uma batata e um refrigerante")
            .Must(items => items == null || items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Não são permitidos itens duplicados no mesmo pedido");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que 0");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("A quantidade deve ser maior que 0")
                .Equal(1).WithMessage("Cada item do pedido deve ter quantidade igual a 1");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("O preço unitário deve ser maior que 0");
        });
    }
}

/// <summary>
/// Validator for UpdateOrderStatusDto
/// </summary>
public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("O status é obrigatório")
            .Must(status => OrderStatus.TryNormalize(status, out _))
            .WithMessage($"O status deve ser um dos seguintes: {string.Join(", ", OrderStatus.AllowedStatuses)}");
    }
}
