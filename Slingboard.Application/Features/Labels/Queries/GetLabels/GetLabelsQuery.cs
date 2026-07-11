using Mediator;

namespace Slingboard.Application.Features.Labels.Queries.GetLabels;

public record GetLabelsQuery(Guid BoardId) : IRequest<List<LabelResponse>>;