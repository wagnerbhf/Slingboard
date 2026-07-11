using Mediator;
using Slingboard.Application.Common.Exceptions;
using Slingboard.Application.Common.Interfaces;
using Slingboard.Domain.Entities;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Application.Features.Auth.Register;

public class RegisterCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async ValueTask<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var emailExists = context.Users.Any(u => u.Email == email);
        if (emailExists)
            throw new ConflictException("Este email já está cadastrado.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, email, passwordHash);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.Name, user.Email.Value);
    }
}