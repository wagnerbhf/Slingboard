namespace Slingboard.Application.Features.Exports;

public record BoardExportData(
    string BoardTitle,
    DateTime GeneratedAt,
    int TotalTasks,
    int MemberCount,
    List<ExportColumnData> Columns);

public record ExportColumnData(string Title, List<ExportTaskRow> Tasks);

public record ExportTaskRow(
    Guid TaskId,
    string Title,
    string? Description,
    string Column,
    string Priority,
    DateTime? DueDate,
    string? AssigneeName,
    string? AssigneeEmail,
    string Labels,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Order,
    string Status,
    List<(string Name, string Color)> LabelBadges);

public record ExportFileResult(byte[] Content, string ContentType, string FileName);