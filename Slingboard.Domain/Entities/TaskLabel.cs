namespace Slingboard.Domain.Entities;

public class TaskLabel
{
    public Guid TaskId { get; private set; }
    public Guid LabelId { get; private set; }

    private TaskLabel() { } // EF Core

    internal static TaskLabel Create(Guid taskId, Guid labelId)
        => new() { TaskId = taskId, LabelId = labelId };
}