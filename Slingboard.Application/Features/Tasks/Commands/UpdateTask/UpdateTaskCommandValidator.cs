using FluentValidation;
using Slingboard.Domain.Enums;

namespace Slingboard.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).Must(p => Enum.TryParse<TaskPriority>(p, out _))
            .WithMessage("Priority inválida. Use: Low, Medium, High ou Urgent.");
    }
}