namespace Modules.Records.UI.Printing;

/// <summary>
/// Resolves a citation (by its short record number) into a fully-formatted
/// <see cref="CitationTexasPrintModel"/> ready for the TX17-4R PDF. Returns <c>null</c> when no
/// citation with that record number is visible to the caller.
/// </summary>
public interface ICitationTexasPrintModelBuilder
{
    Task<CitationTexasPrintModel?> BuildAsync(long recordNumber, CancellationToken cancellationToken = default);
}
