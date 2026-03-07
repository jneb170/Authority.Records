namespace Modules.Records.UI.ViewModels;

public sealed class CreateCitationViewModel
{
    public string Description { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.Today;
    public string CitationNum { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        Description = string.Empty;
        IssueDate = DateTime.Today;
        CitationNum = string.Empty;
        ErrorMessage = null;
    }
}

