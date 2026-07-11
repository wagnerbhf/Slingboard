using Slingboard.Domain.Common;
using Slingboard.Domain.Enums;
using Slingboard.Domain.Events;
using Slingboard.Domain.Exceptions;

namespace Slingboard.Domain.Entities;

public class TaskItem : Entity
{
    public Guid BoardId { get; private set; }
    public Guid ColumnId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; private set; }
    public int Order { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CreatedById { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<TaskLabel> _labels = [];
    public IReadOnlyCollection<TaskLabel> Labels => _labels.AsReadOnly();

    private TaskItem() { } // EF Core

    public static TaskItem Create(
        Guid boardId,
        Guid columnId,
        string title,
        Guid createdById,
        int order,
        string? description = null,
        TaskPriority priority = TaskPriority.Medium,
        DateTime? dueDate = null,
        Guid? assigneeId = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            throw new BusinessRuleViolationException("Título da task deve ter até 200 caracteres.");

        var task = new TaskItem
        {
            BoardId = boardId,
            ColumnId = columnId,
            Title = title.Trim(),
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            AssigneeId = assigneeId,
            CreatedById = createdById,
            Order = order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        task.RaiseDomainEvent(new TaskCreatedDomainEvent(task.Id, boardId));
        return task;
    }

    public void MoveTo(Guid newColumnId, int newOrder, Guid movedByUserId)
    {
        var oldColumnId = ColumnId;
        ColumnId = newColumnId;
        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskMovedDomainEvent(Id, oldColumnId, newColumnId, newOrder, movedByUserId));
    }

    public void UpdateDetails(string title, string? description, TaskPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
            throw new BusinessRuleViolationException("Título da task deve ter até 200 caracteres.");

        Title = title.Trim();
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid? userId)
    {
        AssigneeId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLabel(Guid labelId)
    {
        if (_labels.Any(l => l.LabelId == labelId)) return;
        _labels.Add(TaskLabel.Create(Id, labelId));
    }

    public void RemoveLabel(Guid labelId)
        => _labels.RemoveAll(l => l.LabelId == labelId);
}