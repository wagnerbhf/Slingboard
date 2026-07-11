using FluentValidation;

namespace Slingboard.Application.Features.Labels.Commands.CreateLabel;

public class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    public CreateLabelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da label é obrigatório.")
            .MaximumLength(30).WithMessage("Nome deve ter até 30 caracteres.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Cor é obrigatória.")
            .Matches("^#([A-Fa-f0-9]{6})$").WithMessage("Cor deve estar no formato HEX (#RRGGBB).");
    }
}