namespace Modules.Records.UI.ViewModels;

public sealed class CreateArrestViewModel
{
    public DateTime ArrestedAt { get; set; } = DateTime.Today;
    public string ArrestNum { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        ArrestedAt = DateTime.Today;
        ArrestNum = string.Empty;
        ErrorMessage = null;
    }
}

