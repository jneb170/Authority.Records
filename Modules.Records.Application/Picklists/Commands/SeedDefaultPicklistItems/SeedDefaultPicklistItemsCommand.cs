using MediatR;

namespace Modules.Records.Application.Picklists.Commands.SeedDefaultPicklistItems;

/// <summary>
/// Seeds system default items for the given picklist type for the current agency,
/// skipping any that already exist (by Value). Safe to call multiple times.
/// </summary>
public sealed record SeedDefaultPicklistItemsCommand(string PicklistType) : IRequest;
