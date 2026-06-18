using System.Data;
using System.Text;

namespace Ravelaso.UiPath.Extensions;

public class CsvReadOptions
{
    /// <summary>Whether the first row contains column headers. Defaults to true.</summary>
    public bool UseHeaders { get; set; } = true;

    /// <summary>The column delimiter character. Defaults to ','.</summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>The file encoding. Defaults to UTF-8.</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}

public static class RavelasoCsv
{
    public static DataTable ToDataTable(string filePath)
    {
        return ToDataTable(filePath, new CsvReadOptions());
    }

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
}