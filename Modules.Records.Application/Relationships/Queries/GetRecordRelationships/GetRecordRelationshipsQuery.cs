using MediatR;

namespace Modules.Records.Application.Relationships.Queries.GetRecordRelationships;

public sealed record GetRecordRelationshipsQuery(string RecordType, long RecordNumber)
    : IRequest<RecordRelationshipsDto?>;
