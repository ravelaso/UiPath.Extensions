namespace Ravelaso.UiPath.Extensions.Excel;

/// <summary>
///     Options for writing Excel files with <see cref="ExcelWriter"/>.
/// </summary>
public class ExcelWriterOptions
{
    /// <summary>The sheet name. Defaults to <c>null</c> (uses "Sheet1").</summary>
    public string? SheetName { get; set; }

    /// <summary>Whether to write column names as the first row. Defaults to true.</summary>
    public bool UseHeaders { get; set; } = true;
}
