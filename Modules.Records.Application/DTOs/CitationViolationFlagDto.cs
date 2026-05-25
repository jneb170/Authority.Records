using Modules.Records.Domain.Common.Violations;

namespace Modules.Records.Application.DTOs;

/// <summary>
/// A single structured violation flag set on a citation, with provenance. <see cref="Source"/> is
/// Manual for officer-ticked boxes; Charge (with <see cref="SourceChargeLinkId"/>) is reserved for a
/// future charge-derivation enhancement.
/// </summary>
public sealed record CitationViolationFlagDto(
    ViolationFlagKey Key,
    ViolationFlagSource Source,
    Guid? SourceChargeLinkId);
