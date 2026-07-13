Ravelaso.UiPath.Extensions
---
Utility library for UiPath coded workflows and activities.

## RavelasoUtils

Static helpers for common parsing and normalization tasks.

### ParseCurrency

Parses a string that may contain currency symbols, thousand separators, or EU/US decimal formats and returns the numeric value.

```csharp
double amount = RavelasoUtils.ParseCurrency("$1,136.56");    // 1136.56
double amount = RavelasoUtils.ParseCurrency("1.136,56 €");   // 1136.56
double amount = RavelasoUtils.ParseCurrency("1,136.56");     // 1136.56
```

### ParseDate

Normalizes a date string from EU or US format to `M/d/yyyy`.

```csharp
string date = RavelasoUtils.ParseDate("31/12/2025");                  // "12/31/2025"
string date = RavelasoUtils.ParseDate("12/31/2025");                  // "12/31/2025"
string date = RavelasoUtils.ParseDate("31/12/2025", "yyyy-MM-dd");   // "2025-12-31"
```

### NormalizeString

Uppercases the input and removes all whitespace and line breaks.

```csharp
string clean = RavelasoUtils.NormalizeString("  Hello \n World  "); // "HELLOWORLD"
```

### GetWeekNumber

Returns the current ISO 8601 week number.

```csharp
int week = RavelasoUtils.GetWeekNumber(); // e.g. 28
```

---

## Excel

Reading and writing Excel files using [ClosedXML](https://github.com/ClosedXML/ClosedXML).

### ExcelReader

```csharp
using Ravelaso.UiPath.Extensions.Excel;

// Read first sheet with headers and type inference (default)
DataTable table = ExcelReader.ToDataTable("report.xlsx");

// Read a specific sheet, no headers
var options = new ExcelReadOptions { SheetName = "Sheet2", UseHeaders = false };
DataTable table = ExcelReader.ToDataTable("report.xlsx", options);
```

### ExcelWriter

```csharp
using Ravelaso.UiPath.Extensions.Excel;

// Write with default options (Sheet1, with headers)
ExcelWriter.FromDataTable(table, "output.xlsx");

// Write with custom sheet name, no headers
var options = new ExcelWriterOptions { SheetName = "Data", UseHeaders = false };
ExcelWriter.FromDataTable(table, "output.xlsx", options);
```

### DtUtils

Utilities for transforming and projecting `DataTable` objects.

#### ImportRows

Imports rows from a source `DataTable` into the structure of a target `DataTable`. Columns that don't exist in the source are filled with `DBNull`.

```csharp
using Ravelaso.UiPath.Extensions.Excel;

DataTable target = GetTargetSchema(); // your predefined schema
DataTable source = ExcelReader.ToDataTable("data.xlsx");

DataTable result = DtUtils.ImportRows(target, source);
```

#### GetOnlyColumns

Returns a new `DataTable` with only the specified columns, by index or by name.

```csharp
DataTable filtered = DtUtils.GetOnlyColumns(table, 0, 2);         // by index
DataTable filtered = DtUtils.GetOnlyColumns(table, "Name", "Age"); // by name
DataTable filtered = DtUtils.GetOnlyColumns(table, 0, "Email");   // mixed
```

#### GetDataTable\<T\>

Converts any `IEnumerable<T>` into a `DataTable`. Uses `[Description]` attributes as column names when present.

```csharp
var items = new List<Order> { new() { Id = 1, Total = 99.5m } };
DataTable table = DtUtils.GetDataTable(items);
```

---

## RavelasoCsv

Reading and writing CSV files with proper quoted-field handling.

### Read

```csharp
using Ravelaso.UiPath.Extensions;

// Default: comma delimiter, UTF-8, with headers
DataTable table = RavelasoCsv.ToDataTable("data.csv");

// Custom delimiter and encoding
var options = new CsvReadOptions
{
    Delimiter = ';',
    Encoding = System.Text.Encoding.UTF8,
    UseHeaders = true
};
DataTable table = RavelasoCsv.ToDataTable("data.csv", options);
```

### Write

```csharp
using Ravelaso.UiPath.Extensions;

// Default: comma delimiter, UTF-8, with headers
RavelasoCsv.FromDataTable(table, "output.csv");

// Custom delimiter, no headers
var options = new CsvWriteOptions { Delimiter = ';', UseHeaders = false };
RavelasoCsv.FromDataTable(table, "output.csv", options);
```

---

## QueueHelper

Helpers for building `DataTable` rows from UiPath Queue `SpecificContent`.

### Create a table

```csharp
using Ravelaso.UiPath.Extensions;

DataTable queueTable = QueueHelper.CreateTable("Invoices",
    QueueHelper.Col("InvoiceNumber"),
    QueueHelper.Col("Amount", typeof(decimal)),
    QueueHelper.Col("Vendor")
);
```

### Map SpecificContent to a DataRow

```csharp
IDictionary<string, object> specificContent = queueItem.SpecificContent;
DataRow row = QueueHelper.CreateRowFromSpecificContent(specificContent, queueTable);
queueTable.Rows.Add(row);
```

### Extract typed values

```csharp
string vendor = QueueHelper.GetString(specificContent, "Vendor");
object amount = QueueHelper.GetDecimalOrDbNull(specificContent, "Amount");
```

---

## License

MIT
