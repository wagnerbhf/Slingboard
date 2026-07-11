using FluentValidation;

namespace Slingboard.Application.Features.Columns.Commands.UpdateColumn;

public class UpdateColumnCommandValidator : AbstractValidator<UpdateColumnCommand>
{
    public UpdateColumnCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Limit).GreaterThan(0).When(x => x.Limit.HasValue);
    }
}