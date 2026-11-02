using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.ViewModels;

/// <summary>
/// Edit-mode model for the Incident detail page. DataAnnotations mirror the
/// <c>IncidentDetails</c> value-object guards so the user gets inline,
/// field-level feedback before the save round-trip. Server-side
/// <c>DomainException</c>s that slip past these are mapped back onto the same
/// fields via a <c>ValidationMessageStore</c> in the page.
/// </summary>
public sealed class EditIncidentViewModel
{
    [Required(ErrorMessage = "Incident Number is required.")]
    [MaxLength(30, ErrorMessage = "Incident Number must not exceed 30 characters.")]
    public string IncidentNum { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "CFS Number must not exceed 30 characters.")]
    public string CFSNum { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Local Number must not exceed 30 characters.")]
    public string LocalNum { get; set; } = string.Empty;
}
