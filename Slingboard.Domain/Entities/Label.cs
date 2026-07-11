using Slingboard.Domain.Exceptions;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Entities;

public class Label
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BoardId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public HexColor Color { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private Label() { }

    internal static Label Create(Guid boardId, string name, HexColor color)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 30)
            throw new BusinessRuleViolationException("Nome da label deve ter até 30 caracteres.");

        return new Label
        {
            BoardId = boardId,
            Name = name.Trim(),
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, HexColor color)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 30)
            throw new BusinessRuleViolationException("Nome da label deve ter até 30 caracteres.");

        Name = name.Trim();
        Color = color;
    }
}