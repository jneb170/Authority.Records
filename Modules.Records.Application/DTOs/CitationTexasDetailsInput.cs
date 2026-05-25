namespace Modules.Records.Application.DTOs;

/// <summary>Texas UTC-form-specific artifacts for a citation save. Jurisdiction-neutral offense data is in <see cref="CitationOffenseDetailsInput"/>.</summary>
public sealed record CitationTexasDetailsInput(
    string? DocketNumber = null,
    string? PageNumber = null);
