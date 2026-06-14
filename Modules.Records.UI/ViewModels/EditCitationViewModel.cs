using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.ViewModels;

/// <summary>
/// Edit-mode model for the core fields of the Citation detail page (citation
/// number, description, issue date). DataAnnotations mirror the
/// <c>SaveCitationPageCommandValidator</c> length rules so the user gets inline,
/// field-level feedback before the save round-trip. The supplemental Texas /
/// vehicle / officer / offense field groups are not bound here — they remain on
/// the page as-is for now.
/// </summary>
public sealed class EditCitationViewModel
{
    [MaxLength(50, ErrorMessage = "Citation Number must not exceed 50 characters.")]
    public string CitationNum { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; } = DateTime.Today;
}
