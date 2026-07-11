using Mediator;

namespace Slingboard.Application.Features.Auth.Register;

public record RegisterCommand(string Name, string Email, string Password) : IRequest<RegisterResponse>;