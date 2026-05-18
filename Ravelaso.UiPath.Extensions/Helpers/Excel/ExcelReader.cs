using System.Data;
using ClosedXML.Excel;

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

        using var workbook = new XLWorkbook(filePath);
        var worksheet = GetWorksheet(workbook, options.SheetName);
        var rows = worksheet.RowsUsed().ToList();

        if (rows.Count == 0)
            return dataTable;

        var typeInferenceRow = options.UseHeaders && rows.Count > 1 ? rows[1] : rows[0];
        var headerRow = options.UseHeaders ? rows[0] : null;

        var columnCount = typeInferenceRow.Cells().Count();
        for (int i = 0; i < columnCount; i++)
        {
            var columnName = headerRow != null
                ? headerRow.Cell(i + 1).GetString()
                : $"Column{i + 1}";

            var columnType = options.InferTypes
                ? InferColumnType(typeInferenceRow.Cell(i + 1))
                : typeof(string);

            dataTable.Columns.Add(columnName, columnType);
        }

        var dataStartIndex = options.UseHeaders ? 1 : 0;
        for (int rowIndex = dataStartIndex; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var dataRow = dataTable.NewRow();

            for (int colIndex = 0; colIndex < columnCount; colIndex++)
            {
                dataRow[colIndex] = GetCellValue(row.Cell(colIndex + 1), dataTable.Columns[colIndex].DataType, options.InferTypes);
            }

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    private static IXLWorksheet GetWorksheet(IXLWorkbook workbook, string? sheetName)
    {
        if (sheetName != null)
        {
            var worksheet = workbook.Worksheet(sheetName);
            if (worksheet == null)
                throw new ArgumentException($"Sheet '{sheetName}' not found.");
            return worksheet;
        }

        return workbook.Worksheets.First();
    }

    private static Type InferColumnType(IXLCell cell)
    {
        if (cell.DataType == XLDataType.DateTime)
            return typeof(DateTime);

        if (cell.DataType == XLDataType.Boolean)
            return typeof(bool);

        if (cell.DataType == XLDataType.Number)
        {
            if (IsDateFormat(cell))
                return typeof(DateTime);
            return typeof(double);
        }

        if (cell.DataType == XLDataType.Text)
            return typeof(string);

        if (cell.DataType == XLDataType.Error)
            return typeof(string);

        return typeof(string);
    }

    private static bool IsDateFormat(IXLCell cell)
    {
        var numberFormatId = cell.Style.NumberFormat.NumberFormatId;
        return IsDateNumberFormatId(numberFormatId);
    }

    private static bool IsDateNumberFormatId(int numberFormatId)
    {
        return numberFormatId is
            14 or 15 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or
            45 or 46 or 47 or 50 or 51 or 52 or 53 or 54 or 55 or 56
            or 57 or 58 or 165 or 166 or 167 or 168 or 169 or 170 or 171
            or 172 or 173 or 174 or 175 or 176 or 177 or 178 or 179 or 180
            or 181 or 182 or 183 or 184 or 185 or 186 or 187 or 188 or 189
            or 190 or 191 or 192 or 193 or 194 or 195 or 196 or 197 or 198 or 199;
    }

    private static object GetCellValue(IXLCell cell, Type columnType, bool inferTypes)
    {
        if (cell.IsEmpty())
            return DBNull.Value;

        if (!inferTypes)
            return cell.GetString();

        if (columnType == typeof(DateTime))
        {
            try
            {
                return cell.GetDateTime();
            }
            catch
            {
                return DBNull.Value;
            }
        }

        if (columnType == typeof(bool))
            return cell.GetBoolean();

        if (columnType == typeof(double))
            return cell.GetDouble();

        if (columnType == typeof(string))
            return cell.GetString();

        return cell.GetString();
    }
}