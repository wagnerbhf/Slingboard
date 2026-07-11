namespace Slingboard.Application.Features.Boards;

public record ColumnDto(Guid Id, string Title, int Order, int? Limit);

public record MemberDto(Guid UserId, string Name, string Email, string Role);

public record BoardDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid OwnerId,
    string? BackgroundColor,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<ColumnDto> Columns,
    IReadOnlyCollection<MemberDto> Members);

public record BoardSummaryResponse(
    Guid Id,
    string Title,
    string? Description,
    string? BackgroundColor,
    int TaskCount,
    int MemberCount,
    DateTime UpdatedAt);