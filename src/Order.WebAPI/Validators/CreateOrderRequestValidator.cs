using FluentValidation;
using Order.WebAPI.Dtos.Requests;

namespace Order.WebAPI.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ResellerId)
            .NotEmpty()
            .WithMessage("ResellerId is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");

        RuleFor(x => x.Products)
            .NotEmpty()
            .WithMessage("At least one item is required")
            .Must(items => items is { Count: > 0 })
            .WithMessage("At least one item is required");

        RuleForEach(x => x.Products)
            .SetValidator(new CreateOrderItemRequestValidator());
    }
}

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
    }
}
