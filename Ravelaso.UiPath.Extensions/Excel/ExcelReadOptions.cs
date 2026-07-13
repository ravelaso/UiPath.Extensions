namespace Ravelaso.UiPath.Extensions.Excel;

/// <summary>
///     Options for reading Excel files with <see cref="ExcelReader"/>.
/// </summary>
public class ExcelReadOptions
{
    /// <summary>The sheet name to read. If <c>null</c>, the first sheet is used.</summary>
    public string? SheetName { get; set; }

    /// <summary>Whether the first row contains column headers. Defaults to false.</summary>
    public bool UseHeaders { get; set; }

    /// <summary>Whether to infer column types from cell values. Defaults to true.</summary>
    public bool InferTypes { get; set; } = true;
}