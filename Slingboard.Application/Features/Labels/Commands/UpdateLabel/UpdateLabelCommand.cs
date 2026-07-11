using Mediator;

namespace Slingboard.Application.Features.Labels.Commands.UpdateLabel;

public record UpdateLabelCommand(Guid LabelId, string Name, string Color) : IRequest<LabelResponse>;