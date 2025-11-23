using FluentValidation;
using Order.WebAPI.Dtos.Requests;

namespace Order.WebAPI.Validators;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50);
    }
}

