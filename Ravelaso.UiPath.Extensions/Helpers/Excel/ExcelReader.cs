using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Ravelaso.UiPath.Extensions.Helpers.Excel;

public static class ExcelReader
{
    public static DataTable ToDataTable(string filePath)
    {
        return ToDataTable(filePath, new ExcelReadOptions());
    }

    public static DataTable ToDataTable(string filePath, ExcelReadOptions options)
    {
        var dataTable = new DataTable();

        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Invalid Excel file.");
        var sheet = GetSheet(workbookPart, options.SheetName);
        var sheetId = sheet.Id?.Value ?? throw new InvalidOperationException("Sheet has no Id.");
        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheetId);
        var sheetData = worksheetPart.Worksheet?.GetFirstChild<SheetData>()
            ?? throw new InvalidOperationException("Worksheet has no SheetData.");

        var rows = sheetData.Elements<Row>().ToList();

        if (rows.Count == 0)
            return dataTable;

        var firstRow = rows[0];
        var cellCount = firstRow.Elements<Cell>().Count();

        for (int i = 0; i < cellCount; i++)
        {
            dataTable.Columns.Add(options.UseHeaders ? GetCellValueAsString(workbookPart, firstRow.Elements<Cell>().ElementAt(i)) : $"Column{i + 1}");
        }

        var dataStartIndex = options.UseHeaders ? 1 : 0;

        for (int rowIndex = dataStartIndex; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var dataRow = dataTable.NewRow();

            for (int colIndex = 0; colIndex < cellCount; colIndex++)
            {
                var cell = row.Elements<Cell>().ElementAtOrDefault(colIndex);
                dataRow[colIndex] = GetCellValue(workbookPart, cell, options.InferTypes);
            }

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    private static Sheet GetSheet(WorkbookPart workbookPart, string? sheetName)
    {
        var workbook = workbookPart.Workbook;
        var sheetsCollection = workbook?.Sheets;
        if (sheetsCollection == null)
            throw new InvalidOperationException("Workbook has no Sheets.");

        var sheets = sheetsCollection.Elements<Sheet>().ToList();

        if (sheetName != null)
        {
            var sheet = sheets.FirstOrDefault(s => s.Name == sheetName);
            if (sheet == null)
                throw new ArgumentException($"Sheet '{sheetName}' not found.");
            return sheet;
        }

        return sheets.First() ?? throw new InvalidOperationException("No sheets found in workbook.");
    }

    private static string GetCellValueAsString(WorkbookPart workbookPart, Cell? cell)
    {
        return GetCellValue(workbookPart, cell, inferTypes: false)?.ToString() ?? string.Empty;
    }

    private static object? GetCellValue(WorkbookPart workbookPart, Cell? cell, bool inferTypes)
    {
        if (cell == null)
            return inferTypes ? DBNull.Value : string.Empty;

        var value = cell.InnerText;

        if (cell.DataType == null)
        {
            if (string.IsNullOrEmpty(value))
                return inferTypes ? DBNull.Value : string.Empty;

            if (inferTypes && TryParseNumber(value, out var number))
                return number;

            return inferTypes ? DBNull.Value : value;
        }

        if (cell.DataType == CellValues.SharedString)
            return GetSharedStringValue(workbookPart, int.Parse(value));
        if (cell.DataType == CellValues.Boolean)
            return value == "1";
        if (cell.DataType == CellValues.Error)
            return value;
        return value;
    }

    private static string GetSharedStringValue(WorkbookPart workbookPart, int index)
    {
        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
        var sharedStrings = sharedStringTable?.Elements<SharedStringItem>().ToList();
        return sharedStrings?.ElementAtOrDefault(index)?.InnerText ?? string.Empty;
    }

    private static bool TryParseNumber(string value, out object result)
    {
        result = null!;

        if (double.TryParse(value, out var d))
        {
            result = d;
            if (d == Math.Floor(d) && d >= int.MinValue && d <= int.MaxValue)
                result = (int)d;
            return true;
        }

        return false;
    }
}