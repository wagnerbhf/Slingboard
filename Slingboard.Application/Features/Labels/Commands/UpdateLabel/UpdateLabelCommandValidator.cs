using FluentValidation;

namespace Slingboard.Application.Features.Labels.Commands.UpdateLabel;

public class UpdateLabelCommandValidator : AbstractValidator<UpdateLabelCommand>
{
    public UpdateLabelCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Color).Matches("^#([A-Fa-f0-9]{6})$").WithMessage("Cor deve estar no formato HEX (#RRGGBB).");
    }
}