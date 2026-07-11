using Slingboard.Domain.Common;
using Slingboard.Domain.Enums;
using Slingboard.Domain.Exceptions;
using Slingboard.Domain.ValueObjects;

namespace Slingboard.Domain.Entities;

public class Board : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public HexColor? BackgroundColor { get; private set; }
    public string? BackgroundImage { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<BoardMember> _members = [];
    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();

    private readonly List<Column> _columns = [];
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();

    private readonly List<Label> _labels = [];
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

    private Board() { }

    public static Board Create(string title, Guid ownerId, string? description = null, HexColor? backgroundColor = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
            throw new BusinessRuleViolationException("Título do board deve ter até 100 caracteres.");

        var board = new Board
        {
            Title = title.Trim(),
            Description = description,
            OwnerId = ownerId,
            BackgroundColor = backgroundColor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        board._members.Add(BoardMember.Create(board.Id, ownerId, BoardMemberRole.Owner));

        board._columns.Add(Column.Create(board.Id, "To Do", 0));
        board._columns.Add(Column.Create(board.Id, "In Progress", 1));
        board._columns.Add(Column.Create(board.Id, "Done", 2));

        return board;
    }

    public void UpdateDetails(string title, string? description, HexColor? backgroundColor)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
            throw new BusinessRuleViolationException("Título do board deve ter até 100 caracteres.");

        Title = title.Trim();
        Description = description;
        BackgroundColor = backgroundColor;
        UpdatedAt = DateTime.UtcNow;
    }

    public Column AddColumn(string title, int? limit = null)
    {
        var order = _columns.Count == 0 ? 0 : _columns.Max(c => c.Order) + 1;
        var column = Column.Create(Id, title, order, limit);
        _columns.Add(column);
        UpdatedAt = DateTime.UtcNow;
        return column;
    }

    public void RemoveColumn(Guid columnId)
    {
        if (_columns.Count <= 1)
            throw new BusinessRuleViolationException("O board deve ter pelo menos uma coluna.");

        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new BusinessRuleViolationException("Coluna não encontrada.");

        _columns.Remove(column);
        UpdatedAt = DateTime.UtcNow;
    }

    public BoardMember AddMember(Guid userId, BoardMemberRole role)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new BusinessRuleViolationException("Usuário já é membro deste board.");

        var member = BoardMember.Create(Id, userId, role);
        _members.Add(member);
        UpdatedAt = DateTime.UtcNow;
        return member;
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == OwnerId)
            throw new BusinessRuleViolationException("O Owner não pode ser removido do board.");

        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new BusinessRuleViolationException("Membro não encontrado.");

        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsMember(Guid userId) => _members.Any(m => m.UserId == userId);

    public Label AddLabel(string name, HexColor color)
    {
        if (_labels.Any(l => l.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new BusinessRuleViolationException("Já existe uma label com esse nome neste board.");

        var label = Label.Create(Id, name, color);
        _labels.Add(label);
        UpdatedAt = DateTime.UtcNow;
        return label;
    }

    public void RemoveLabel(Guid labelId)
    {
        var label = _labels.FirstOrDefault(l => l.Id == labelId)
            ?? throw new BusinessRuleViolationException("Label não encontrada.");

        _labels.Remove(label);
        UpdatedAt = DateTime.UtcNow;
    }
}