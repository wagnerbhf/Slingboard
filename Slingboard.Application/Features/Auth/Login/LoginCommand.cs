using Mediator;

namespace Slingboard.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;