using FluentValidation;
using Slingboard.Domain.Enums;

namespace Slingboard.Application.Features.Boards.Commands.AddMember;

public class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Role)
            .Must(role => Enum.TryParse<BoardMemberRole>(role, out _))
            .WithMessage("Role inválida. Use: Owner, Admin ou Member.");
    }
}