using FluentValidation;

namespace Slingboard.Application.Features.Columns.Commands.CreateColumn;

public class CreateColumnCommandValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título da coluna é obrigatório.")
            .MaximumLength(100).WithMessage("Título deve ter até 100 caracteres.");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limite deve ser maior que zero.")
            .When(x => x.Limit.HasValue);
    }
}