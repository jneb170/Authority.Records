namespace Modules.Records.UI.ViewModels;

public sealed class CreateCitationViewModel
{
    public Guid IncidentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.Today;
    public string? ErrorMessage { get; set; }

    public void Reset()
    {
        Description = string.Empty;
        IssueDate = DateTime.Today;
        ErrorMessage = null;
    }
}
