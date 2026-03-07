using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Names.Queries.GetNameByRecordNumber;

public sealed record GetNameByRecordNumberQuery(long RecordNumber) : IRequest<NameDto?>;
