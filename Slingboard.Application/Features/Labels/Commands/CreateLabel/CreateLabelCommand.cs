using Mediator;

namespace Slingboard.Application.Features.Labels.Commands.CreateLabel;

public record CreateLabelCommand(Guid BoardId, string Name, string Color) : IRequest<LabelResponse>;