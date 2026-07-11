using Mediator;

namespace Slingboard.Application.Features.Labels.Commands.DeleteLabel;

public record DeleteLabelCommand(Guid LabelId) : IRequest<Unit>;