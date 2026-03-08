using MediatR;

namespace Modules.Records.Application.Admin.Commands.RebuildReadModels;

public sealed record RebuildReadModelsCommand : IRequest<RebuildReadModelsResult>;
