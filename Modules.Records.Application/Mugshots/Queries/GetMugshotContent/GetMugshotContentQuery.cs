using MediatR;

namespace Modules.Records.Application.Mugshots.Queries.GetMugshotContent;

/// <summary>
/// Opens a mugshot's image for streaming. Returns null when the mugshot does not exist,
/// is soft-deleted, belongs to another tenant (query filters), or its file is missing.
/// </summary>
public sealed record GetMugshotContentQuery(Guid MugshotId) : IRequest<MugshotContent?>;

/// <summary>An open mugshot image. The caller owns <see cref="Content"/> and must dispose it.</summary>
public sealed record MugshotContent(Stream Content, string ContentType);
