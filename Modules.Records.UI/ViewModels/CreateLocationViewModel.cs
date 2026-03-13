using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.ViewModels;

public sealed class CreateLocationViewModel
{
    [Required(ErrorMessage = "Street Name is required.")]
    [MaxLength(200)]
    public string  StreetAddress   { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100)]
    public string  City            { get; set; } = string.Empty;
    public string? StreetNumber    { get; set; }
    public Guid?   PreDirectionId  { get; set; }
    public Guid?   StreetTypeId    { get; set; }
    public Guid?   PostDirectionId { get; set; }
    public Guid?   StateId         { get; set; }
    public Guid?   CountryId       { get; set; }
    public string? Zip             { get; set; }
    public string? AptSuite        { get; set; }
    public string? Coordinates     { get; set; }
    public string? CommonPlaceName { get; set; }
    public string? Comments        { get; set; }

    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        StreetAddress   = string.Empty;
        City            = string.Empty;
        StreetNumber    = null;
        PreDirectionId  = null;
        StreetTypeId    = null;
        PostDirectionId = null;
        StateId         = null;
        CountryId       = null;
        Zip             = null;
        AptSuite        = null;
        Coordinates     = null;
        CommonPlaceName = null;
        Comments        = null;
        ErrorMessage    = null;
    }
}
