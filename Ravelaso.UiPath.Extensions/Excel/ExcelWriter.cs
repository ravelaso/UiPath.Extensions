using System.Data;
using ClosedXML.Excel;

namespace Ravelaso.UiPath.Extensions.Excel;

/// <summary>
///     Writes <see cref="DataTable"/> objects to Excel files using ClosedXML.
/// </summary>
public static class ExcelWriter
{
    /// <summary>
    ///     Writes a <see cref="DataTable"/> to an Excel file using default options (Sheet1, with headers).
    /// </summary>
    /// <param name="table">The DataTable to write.</param>
    /// <param name="filePath">The output file path.</param>
    public static void FromDataTable(DataTable table, string filePath)
    {
        FromDataTable(table, filePath, new ExcelWriterOptions());
    }

    /// <summary>
    ///     Writes a <see cref="DataTable"/> to an Excel file using the specified options.
    /// </summary>
    /// <param name="table">The DataTable to write.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="options">The write options (sheet name, headers).</param>
    public static void FromDataTable(DataTable table, string filePath, ExcelWriterOptions options)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(options.SheetName ?? "Sheet1");

        var startRow = 1;

        if (options.UseHeaders)
        {
            for (int col = 0; col < table.Columns.Count; col++)
            {
                worksheet.Cell(1, col + 1).Value = table.Columns[col].ColumnName;
            }
            startRow = 2;
        }

        for (int row = 0; row < table.Rows.Count; row++)
        {
            for (int col = 0; col < table.Columns.Count; col++)
            {
                var value = table.Rows[row][col];
                worksheet.Cell(startRow + row, col + 1).Value = value == DBNull.Value
                    ? string.Empty
                    : value.ToString();
            }
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }
}
