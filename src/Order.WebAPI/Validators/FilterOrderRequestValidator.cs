using FluentValidation;
using Order.WebAPI.Dtos.Requests;

namespace Order.WebAPI.Validators;

public class FilterOrderRequestValidator : AbstractValidator<FilterOrderRequest>
{
    public FilterOrderRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(20);
    }
}
