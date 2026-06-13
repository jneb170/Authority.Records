using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.ViewModels;

/// <summary>
/// Edit-mode model for the Name detail page. Holds the free-text identity
/// scalars whose length/required rules are enforced by
/// <c>UpdateNameDetailsCommandValidator</c>; the DataAnnotations here mirror
/// those guards so the user gets inline, field-level feedback before the save
/// round-trip. Picklist FKs, phone components, dates, height/weight and the
/// address pickers are not bound here — they keep their existing page handling.
/// </summary>
public sealed class EditNameViewModel
{
    [Required(ErrorMessage = "Last name (or business name) is required.")]
    [MaxLength(250, ErrorMessage = "Last/Business name must not exceed 250 characters.")]
    public string LastOrBusinessName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "First name must not exceed 100 characters.")]
    public string? FirstName { get; set; }

    [MaxLength(100, ErrorMessage = "Middle name must not exceed 100 characters.")]
    public string? MiddleName { get; set; }

    [MaxLength(50, ErrorMessage = "Driver's license number must not exceed 50 characters.")]
    public string? DlNumber { get; set; }

    public string? PlaceOfBirth { get; set; }
    public string? Ssn { get; set; }
    public string? FbiNumber { get; set; }
    public string? LocalNumber { get; set; }
}
