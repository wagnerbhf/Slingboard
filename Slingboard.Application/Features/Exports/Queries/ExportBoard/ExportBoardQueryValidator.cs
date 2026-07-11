using FluentValidation;

namespace Slingboard.Application.Features.Exports.Queries.ExportBoard;

public class ExportBoardQueryValidator : AbstractValidator<ExportBoardQuery>
{
    public ExportBoardQueryValidator()
    {
        RuleFor(x => x.Format)
            .Must(f => f is "csv" or "pdf")
            .WithMessage("Formato inválido. Use 'csv' ou 'pdf'.");
    }
}