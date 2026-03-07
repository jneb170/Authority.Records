using MediatR;
using Modules.Records.Domain.Common;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistTypes;

public sealed class GetPicklistTypesHandler : IRequestHandler<GetPicklistTypesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetPicklistTypesQuery request, CancellationToken cancellationToken)
        => Task.FromResult(PicklistTypes.All);
}
