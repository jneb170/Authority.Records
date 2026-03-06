namespace Modules.Records.UI.ViewModels;

public sealed class CreateArrestViewModel
{
    public Guid IncidentId { get; set; }
    public string SuspectName { get; set; } = string.Empty;
    public DateTime ArrestedAt { get; set; } = DateTime.Today;
    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        SuspectName = string.Empty;
        ArrestedAt = DateTime.Today;
        ErrorMessage = null;
    }
}
