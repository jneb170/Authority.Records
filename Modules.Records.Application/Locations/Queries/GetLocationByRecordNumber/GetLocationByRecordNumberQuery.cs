using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Locations.Queries.GetLocationByRecordNumber;

public sealed record GetLocationByRecordNumberQuery(long RecordNumber) : IRequest<LocationDto?>;
