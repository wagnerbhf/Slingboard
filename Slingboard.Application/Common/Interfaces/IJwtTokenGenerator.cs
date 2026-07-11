using Slingboard.Domain.Entities;

namespace Slingboard.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
}