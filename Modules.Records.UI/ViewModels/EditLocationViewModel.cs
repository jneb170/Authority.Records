using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.ViewModels;

/// <summary>
/// Edit-mode model for the Location detail page. DataAnnotations mirror the
/// <c>CreateLocationCommandValidator</c> guards (Street Name and City are the
/// only required/length-constrained scalars) so the user gets inline,
/// field-level feedback before the save round-trip. Picklist FKs
/// (direction/type/state/country) are not bound here — they stay on the page as
/// their own components and carry no client-side rules.
/// </summary>
public sealed class EditLocationViewModel
{
    [Required(ErrorMessage = "Street Name is required.")]
    [MaxLength(200, ErrorMessage = "Street Name must not exceed 200 characters.")]
    public string StreetAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City must not exceed 100 characters.")]
    public string City { get; set; } = string.Empty;

    public string? StreetNumber    { get; set; }
    public string? Zip             { get; set; }
    public string? AptSuite        { get; set; }
    public string? CommonPlaceName { get; set; }
    public string? Coordinates     { get; set; }
    public string? Address         { get; set; }
    public string? Comments        { get; set; }
}
