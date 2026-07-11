using FluentValidation;

namespace Slingboard.Application.Features.Boards.Commands.UpdateBoard;

public class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(100).WithMessage("Título deve ter até 100 caracteres.");

        RuleFor(x => x.BackgroundColor)
            .Matches("^#([A-Fa-f0-9]{6})$").WithMessage("Cor deve estar no formato HEX (#RRGGBB).")
            .When(x => !string.IsNullOrWhiteSpace(x.BackgroundColor));
    }
}