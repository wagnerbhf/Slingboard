using Slingboard.Domain.Common;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Entities;

public class User : Entity
{
    public Email Email { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(string name, Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome é obrigatório.", nameof(name));

        return new User
        {
            Name = name.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RegisterLogin() => LastLoginAt = DateTime.UtcNow;

    public void Deactivate() => IsActive = false;
}