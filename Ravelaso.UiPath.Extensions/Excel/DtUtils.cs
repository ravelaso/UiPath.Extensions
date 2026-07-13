using System.ComponentModel;
using System.Data;
using System.Reflection;
// ReSharper disable UnusedMember.Global

namespace Ravelaso.UiPath.Extensions.Excel;

public static class DtUtils
{
    /// <summary>
    /// Imports rows from the source DataTable into a new DataTable, retaining the structure of the target DataTable.
    /// If a column in the target DataTable does not exist in the source DataTable, its values will be set to DBNull.
    /// </summary>
    /// <param name="target">
    /// The target DataTable whose structure will be cloned for the new DataTable.
    /// The columns and their data types in the target determine the structure of the resulting DataTable.
    /// </param>
    /// <param name="source">
    /// The source DataTable from which rows will be imported.
    /// Only the columns present in both the source and target will have their data transferred.
    /// </param>
    /// <returns>
    /// A new DataTable containing the structure of the target DataTable and rows of data from the source DataTable.
    /// </returns>
    public static DataTable ImportRows(DataTable target, DataTable source)
    {
        var result = target.Clone();

        foreach (DataRow row in source.Rows)
        {
            var newRow = result.NewRow();
            foreach (DataColumn col in result.Columns)
            {
                newRow[col.ColumnName] = source.Columns.Contains(col.ColumnName)
                    ? row[col.ColumnName]
                    : DBNull.Value;
            }

            result.Rows.Add(newRow);
        }

        return result;
    }

    /// <summary>
    /// Creates a new DataTable that contains only the specified columns from the input DataTable.
    /// Columns can be specified by their index or their name.
    /// </summary>
    /// <param name="dt">
    /// The source DataTable from which columns will be selected.
    /// </param>
    /// <param name="columns">
    /// An array of column identifiers. Each identifier can be an integer, representing the column index,
    /// or a string, representing the column name.
    /// </param>
    /// <returns>
    /// A new DataTable containing only the specified columns and the data from those columns.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an integer column index is out of the range of existing column indices in the source DataTable.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when a specified column name does not exist in the source DataTable, or when the column specification
    /// is neither an integer (index) nor a string (name).
    /// </exception>
    public static DataTable GetOnlyColumns(DataTable dt, params object[] columns)
    {
        var result = new DataTable();

        foreach (var column in columns)
        {
            if (column is int index)
            {
                if (index < 0 || index >= dt.Columns.Count)
                    throw new ArgumentOutOfRangeException(nameof(index),
                        $"Index {index} is out of range. DataTable has {dt.Columns.Count} columns.");
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
                throw new ArgumentException(
                    $"Invalid column specification: {column}. Use int (index) or string (column name).",
                    nameof(columns));
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

    /// <summary>
    /// Converts an enumerable collection of objects into a DataTable.
    /// The DataTable's columns correspond to the public readable properties of the objects.
    /// If a property has a Description attribute, its value is used as the column name; otherwise, the property name is used.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the enumerable collection.</typeparam>
    /// <param name="data">The enumerable collection of objects to be converted into a DataTable.</param>
    /// <returns>
    /// A DataTable where each column corresponds to a public readable property of type <typeparamref name="T"/>
    /// and each row corresponds to an element of the <paramref name="data"/>.
    /// </returns>
    public static DataTable GetDataTable<T>(IEnumerable<T> data)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var type = typeof(T);


        // Support both properties (classes/records) and fields (record structs)
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Select(p => (
                p.Name,
                MemberType: Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType,
                p.GetCustomAttribute<DescriptionAttribute>()?.Description,
                GetValue: (Func<T, object?>)(item => p.GetValue(item))
            ))
            .ToList();

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(f => (
                f.Name,
                MemberType: Nullable.GetUnderlyingType(f.FieldType) ?? f.FieldType,
                f.GetCustomAttribute<DescriptionAttribute>()?.Description,
                GetValue: (Func<T, object?>)(item => f.GetValue(item))
            ))
            .ToList();

        var members = properties.Count > 0 ? properties : fields;

        // Build columns using Description attribute if present, otherwise use member name
        foreach (var member in members)
        {
            var columnName = string.IsNullOrWhiteSpace(member.Description) ? member.Name : member.Description;
            dataTable.Columns.Add(columnName, member.MemberType);
        }

        // Populate rows
        foreach (var item in data)
        {
            var row = dataTable.NewRow();
            foreach (var member in members)
            {
                var columnName = string.IsNullOrWhiteSpace(member.Description) ? member.Name : member.Description;
                row[columnName] = member.GetValue(item) ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}