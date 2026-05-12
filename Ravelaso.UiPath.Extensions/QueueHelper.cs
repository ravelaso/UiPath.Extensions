using System.Data;
using System.Globalization;

namespace Ravelaso.UiPath.Extensions;

public static class QueueHelper
{
    public static DataColumn Col(string name)
    {
        return new(name, typeof(string));
    }

    public static DataColumn Col(string name, Type type)
    {
        return new(name, type);
    }

    public static DataTable CreateTable(string tableName, params DataColumn[] columns)
    {
        var table = new DataTable(tableName);
        table.Columns.AddRange(columns);
        return table;
    }

    public static DataRow CreateRowFromSpecificContent(
        IDictionary<string, object> specificContent, DataTable table)
    {
        var row = table.NewRow();

        foreach (var kvp in specificContent)
        {
            if (!table.Columns.Contains(kvp.Key))
                continue;

            var col = table.Columns[kvp.Key];
            if (col is null) continue;

            var colType = col.DataType;

            row[kvp.Key] = colType == typeof(decimal)
                ? ToDecimalOrDbNull(kvp.Value, kvp.Key)
                : ToStringOrEmpty(kvp.Value);
        }

        return row;
    }

    private static string ToStringOrEmpty(object value)
    {
        if (value is null || value == DBNull.Value)
            return string.Empty;

        return value.ToString() ?? string.Empty;
    }

    private static object ToDecimalOrDbNull(object value, string key)
    {
        if (value is null || value == DBNull.Value)
            return DBNull.Value;

        if (value is decimal d) return d;
        if (value is int i) return Convert.ToDecimal(i);
        if (value is long l) return Convert.ToDecimal(l);
        if (value is double db) return Convert.ToDecimal(db);
        if (value is float f) return Convert.ToDecimal(f);

        var text = value.ToString();

        if (string.IsNullOrWhiteSpace(text))
            return DBNull.Value;

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            return parsed;

        throw new FormatException($"Value for '{key}' could not be converted to decimal.");
    }

    public static string GetString(IDictionary<string, object> specificContent, string key)
    {
        if (!specificContent.TryGetValue(key, out var value))
            return string.Empty;

        return ToStringOrEmpty(value);
    }

    public static object GetDecimalOrDbNull(IDictionary<string, object> specificContent, string key)
    {
        if (!specificContent.TryGetValue(key, out var value))
            return DBNull.Value;

        return ToDecimalOrDbNull(value, key);
    }

    public static DataRow CreateInvoiceRowFromSpecificContent(
        IDictionary<string, object> specificContent, DataTable table)
    {
        return CreateRowFromSpecificContent(specificContent, table);
    }
}