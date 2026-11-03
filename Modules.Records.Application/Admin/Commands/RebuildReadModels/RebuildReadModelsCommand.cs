using MediatR;
using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Admin.Commands.RebuildReadModels;

/// <summary>
/// Clears and repopulates every read model for the jurisdiction. Admin only (#125); the
/// automatic rebuild runs the handler directly as system maintenance, not through this request.
/// </summary>
[RequiresRole(RoleNames.Admin)]
public sealed record RebuildReadModelsCommand : IRequest<RebuildReadModelsResult>;
