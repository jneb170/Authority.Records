namespace Modules.Records.UI.Printing;

/// <summary>
/// Selects the page geometry for the Texas citation PDF. Both variants render the SAME fixed-width
/// content block (so spacing is identical); only the page wrapper differs.
/// </summary>
public enum CitationPrintSize
{
    /// <summary>8.5"x11" Letter. The form sits in a fixed ~4"-wide block anchored to the left, leaving the
    /// right half blank (reserved for printing the back of the ticket on the same page later).</summary>
    Letter,

    /// <summary>4"-wide continuous roll for patrol-car thermal printers — a single page, no page breaks.</summary>
    FourInch,
}
