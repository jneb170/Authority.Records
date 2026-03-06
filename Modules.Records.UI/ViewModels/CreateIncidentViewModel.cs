namespace Modules.Records.UI.ViewModels;

public sealed class CreateIncidentViewModel
{
    public string Description { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        Description = string.Empty;
        ErrorMessage = null;
    }
}
