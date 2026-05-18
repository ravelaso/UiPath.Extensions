using System.Data;

namespace Ravelaso.UiPath.Extensions.Excel;

public static class DataTableExtensions
{
    public static DataTable GetOnlyColumns(this DataTable dt, params object[] columns)
    {
        var result = new DataTable();

        foreach (var column in columns)
        {
            if (column is int index)
            {
                if (index < 0 || index >= dt.Columns.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. DataTable has {dt.Columns.Count} columns.");
                result.Columns.Add(dt.Columns[index].ColumnName, dt.Columns[index].DataType);
            }
            else if (column is string columnName)
            {
                if (!dt.Columns.Contains(columnName))
                    throw new ArgumentException($"Column '{columnName}' not found in DataTable.", nameof(columnName));
                var sourceColumn = dt.Columns[columnName]!;
                result.Columns.Add(columnName, sourceColumn.DataType);
            }
            else
            {
                throw new ArgumentException($"Invalid column specification: {column}. Use int (index) or string (column name).", nameof(columns));
            }
        }

        foreach (DataRow row in dt.Rows)
        {
            var newRow = result.NewRow();
            for (int i = 0; i < result.Columns.Count; i++)
            {
                newRow[i] = row[result.Columns[i].ColumnName];
            }
            result.Rows.Add(newRow);
        }

        return result;
    }
}