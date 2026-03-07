using MediatR;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistTypes;

/// <summary>Returns the list of all known picklist type keys.</summary>
public sealed record GetPicklistTypesQuery : IRequest<IReadOnlyList<string>>;
