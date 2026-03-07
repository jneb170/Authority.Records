using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Picklists.Queries.GetPicklistSetting;

public sealed record GetPicklistSettingQuery(string PicklistType) : IRequest<PicklistSettingDto?>;
