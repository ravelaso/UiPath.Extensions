namespace Ravelaso.UiPath.Extensions.Excel;

public class ExcelReadOptions
{
    public string? SheetName { get; set; }

    public bool UseHeaders { get; set; }

    public bool InferTypes { get; set; } = true;
}