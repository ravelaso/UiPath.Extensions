using System.Data;
using System.Text;

namespace Ravelaso.UiPath.Extensions;

/// <summary>
///     Options for reading CSV files with <see cref="RavelasoCsv"/>.
/// </summary>
public class CsvReadOptions
{
    /// <summary>Whether the first row contains column headers. Defaults to true.</summary>
    public bool UseHeaders { get; set; } = true;

    /// <summary>The column delimiter character. Defaults to ','.</summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>The file encoding. Defaults to UTF-8.</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}

/// <summary>
///     Options for writing CSV files with <see cref="RavelasoCsv"/>.
/// </summary>
public class CsvWriteOptions
{
    /// <summary>The column delimiter character. Defaults to ','.</summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>The file encoding. Defaults to UTF-8.</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>Whether to write column names as the first row. Defaults to true.</summary>
    public bool UseHeaders { get; set; } = true;
}

/// <summary>
///     Reads and writes CSV files with proper quoted-field handling.
/// </summary>
public static class RavelasoCsv
{
    /// <summary>
    ///     Reads a CSV file into a <see cref="DataTable"/> using default options (comma delimiter, UTF-8, with headers).
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    public static DataTable ToDataTable(string filePath)
    {
        return ToDataTable(filePath, new CsvReadOptions());
    }

    /// <summary>
    ///     Reads a CSV file into a <see cref="DataTable"/> using the specified options.
    /// </summary>
    /// <param name="filePath">The path to the CSV file.</param>
    /// <param name="options">The read options (delimiter, encoding, headers).</param>
    public static DataTable ToDataTable(string filePath, CsvReadOptions options)
    {
        var dataTable = new DataTable();
        var lines = File.ReadAllLines(filePath, options.Encoding);

        if (lines.Length == 0)
            return dataTable;

        var rows = lines
            .Select(line => ParseCsvLine(line, options.Delimiter))
            .ToList();

        var headerRow = options.UseHeaders ? rows[0] : null;
        var dataStartIndex = options.UseHeaders ? 1 : 0;
        var columnCount = rows[0].Length;

        for (int i = 0; i < columnCount; i++)
        {
            var columnName = headerRow != null ? headerRow[i] : $"Column{i + 1}";
            dataTable.Columns.Add(columnName, typeof(string));
        }

        for (int rowIndex = dataStartIndex; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var dataRow = dataTable.NewRow();

            for (int colIndex = 0; colIndex < columnCount; colIndex++)
            {
                var value = colIndex < row.Length ? row[colIndex] : string.Empty;
                dataRow[colIndex] = string.IsNullOrEmpty(value) ? DBNull.Value : value;
            }

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    /// <summary>
    ///     Writes a <see cref="DataTable"/> to a CSV file using default options (comma delimiter, UTF-8, with headers).
    /// </summary>
    /// <param name="table">The DataTable to write.</param>
    /// <param name="filePath">The output file path.</param>
    public static void FromDataTable(DataTable table, string filePath)
    {
        FromDataTable(table, filePath, new CsvWriteOptions());
    }

    /// <summary>
    ///     Writes a <see cref="DataTable"/> to a CSV file using the specified options.
    /// </summary>
    /// <param name="table">The DataTable to write.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="options">The write options (delimiter, encoding, headers).</param>
    public static void FromDataTable(DataTable table, string filePath, CsvWriteOptions options)
    {
        var lines = new List<string>();

        if (options.UseHeaders)
        {
            var header = string.Join(options.Delimiter,
                table.Columns.Cast<DataColumn>().Select(c => EscapeField(c.ColumnName, options.Delimiter)));
            lines.Add(header);
        }

        foreach (DataRow row in table.Rows)
        {
            var fields = new List<string>();
            foreach (var item in row.ItemArray)
            {
                var value = item == DBNull.Value || item is null ? string.Empty : item.ToString() ?? string.Empty;
                fields.Add(EscapeField(value, options.Delimiter));
            }
            lines.Add(string.Join(options.Delimiter, fields));
        }

        File.WriteAllLines(filePath, lines, options.Encoding);
    }

    private static string[] ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Check for escaped quote ("")
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }

    private static string EscapeField(string value, char delimiter)
    {
        if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
