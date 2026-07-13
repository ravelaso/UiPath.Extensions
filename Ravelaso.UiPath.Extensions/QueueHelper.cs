using System.Data;
using System.Globalization;

namespace Ravelaso.UiPath.Extensions;

/// <summary>
///     Helpers for building <see cref="DataTable"/> rows from UiPath Queue SpecificContent.
/// </summary>
public static class QueueHelper
{
    /// <summary>
    ///     Creates a <see cref="DataColumn"/> of type <see cref="string"/>.
    /// </summary>
    /// <param name="name">The column name.</param>
    public static DataColumn Col(string name)
    {
        return new(name, typeof(string));
    }

    /// <summary>
    ///     Creates a <see cref="DataColumn"/> of the specified type.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="type">The data type of the column.</param>
    public static DataColumn Col(string name, Type type)
    {
        return new(name, type);
    }

    /// <summary>
    ///     Creates a <see cref="DataTable"/> with the specified name and columns.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columns">The columns to add to the table.</param>
    public static DataTable CreateTable(string tableName, params DataColumn[] columns)
    {
        var table = new DataTable(tableName);
        table.Columns.AddRange(columns);
        return table;
    }

    /// <summary>
    ///     Creates a <see cref="DataRow"/> from a SpecificContent dictionary, mapping matching columns to their values.
    ///     Decimal columns are converted automatically; other columns are stored as strings.
    /// </summary>
    /// <param name="specificContent">The Queue item SpecificContent dictionary.</param>
    /// <param name="table">The target table whose schema defines the row structure.</param>
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

    /// <summary>
    ///     Extracts a string value from a SpecificContent dictionary. Returns <see cref="string.Empty"/> if the key is not found.
    /// </summary>
    /// <param name="specificContent">The Queue item SpecificContent dictionary.</param>
    /// <param name="key">The key to look up.</param>
    public static string GetString(IDictionary<string, object> specificContent, string key)
    {
        if (!specificContent.TryGetValue(key, out var value))
            return string.Empty;

        return ToStringOrEmpty(value);
    }

    /// <summary>
    ///     Extracts a decimal value from a SpecificContent dictionary. Returns <see cref="DBNull.Value"/> if the key is not found or the value cannot be converted.
    /// </summary>
    /// <param name="specificContent">The Queue item SpecificContent dictionary.</param>
    /// <param name="key">The key to look up.</param>
    public static object GetDecimalOrDbNull(IDictionary<string, object> specificContent, string key)
    {
        if (!specificContent.TryGetValue(key, out var value))
            return DBNull.Value;

        return ToDecimalOrDbNull(value, key);
    }

    /// <summary>
    ///     Alias for <see cref="CreateRowFromSpecificContent"/>. Creates a <see cref="DataRow"/> from a SpecificContent dictionary.
    /// </summary>
    /// <param name="specificContent">The Queue item SpecificContent dictionary.</param>
    /// <param name="table">The target table whose schema defines the row structure.</param>
    public static DataRow CreateInvoiceRowFromSpecificContent(
        IDictionary<string, object> specificContent, DataTable table)
    {
        return CreateRowFromSpecificContent(specificContent, table);
    }
}