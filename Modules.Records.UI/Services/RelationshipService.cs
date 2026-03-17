using MediatR;
using Modules.Records.Application.Relationships.Queries.GetRecordRelationships;

namespace Modules.Records.UI.Services;

public sealed class RelationshipService(ISender sender) : IRelationshipService
{
    public Task<RecordRelationshipsDto?> GetAsync(string recordType, long recordNumber) =>
        sender.Send(new GetRecordRelationshipsQuery(recordType, recordNumber));
}
