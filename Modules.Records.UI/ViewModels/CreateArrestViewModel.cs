namespace Modules.Records.UI.ViewModels;

public sealed class CreateArrestViewModel
{
    public string SuspectName { get; set; } = string.Empty;
    public DateTime ArrestedAt { get; set; } = DateTime.Today;
    public string ArrestNum { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        SuspectName = string.Empty;
        ArrestedAt = DateTime.Today;
        ArrestNum = string.Empty;
        ErrorMessage = null;
    }
}

