namespace Modules.Records.Application.DTOs;

/// <summary>Texas UTC-form-specific artifacts returned with a citation. See <see cref="CitationTexasDetailsInput"/>.</summary>
public sealed record CitationTexasDetailsDto(
    string? DocketNumber = null,
    string? PageNumber = null);
