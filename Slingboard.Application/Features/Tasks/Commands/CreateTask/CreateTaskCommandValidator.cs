using FluentValidation;
using Slingboard.Domain.Enums;

namespace Slingboard.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter até 200 caracteres.");

        RuleFor(x => x.Priority)
            .Must(p => Enum.TryParse<TaskPriority>(p, out _))
            .WithMessage("Priority inválida. Use: Low, Medium, High ou Urgent.");

        RuleFor(x => x.ColumnId).NotEmpty();
    }
}