using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Citations.Queries.GetCitationByRecordNumber;

public sealed record GetCitationByRecordNumberQuery(long RecordNumber) : IRequest<CitationDto?>;
