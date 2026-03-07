using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestByRecordNumber;

public sealed record GetArrestByRecordNumberQuery(long RecordNumber) : IRequest<ArrestDto?>;
