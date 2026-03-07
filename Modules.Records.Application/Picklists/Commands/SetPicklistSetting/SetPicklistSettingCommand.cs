using MediatR;

namespace Modules.Records.Application.Picklists.Commands.SetPicklistSetting;

/// <summary>Creates or updates the IsRequired setting for a picklist type for the current agency.</summary>
public sealed record SetPicklistSettingCommand(string PicklistType, bool IsRequired) : IRequest;
