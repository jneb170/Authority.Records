using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.ViewModels;

public sealed class CreateIncidentViewModel
{
    [MaxLength(30, ErrorMessage = "Incident Number must not exceed 30 characters.")]
    public string IncidentNum { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "CFS Number must not exceed 30 characters.")]
    public string CFSNum { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Local Number must not exceed 30 characters.")]
    public string LocalNum { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        Description = string.Empty;
        CFSNum = string.Empty;
        LocalNum = string.Empty;
        ErrorMessage = null;
    }
}
