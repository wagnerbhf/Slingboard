namespace Slingboard.Application.Features.Labels;

public record LabelResponse(Guid Id, Guid BoardId, string Name, string Color);