using System.Globalization;
using System.Text.RegularExpressions;

namespace Ravelaso.UiPath.Extensions;

public static class RavelasoUtils
{
    /// <summary>
    ///     Parses a string that may contain currency symbols, thousand separators,
    ///     or EU/US decimal separators and returns the numeric value.
    /// </summary>
    public static double ParseCurrency(string input)
    {
        // Remove any non-numeric characters except for the decimal and thousands separators
        var cleanedInput = Regex.Replace(input, @"[^\d.,]", "");

        // Determine the format based on the presence of commas and periods
        if (cleanedInput.Contains(',') && cleanedInput.Contains('.'))
        {
            // If both are present, determine which is the decimal separator
            // Assume the last occurrence of ',' is the decimal separator
            // This handles cases like "1,136.56" (US format)
            var lastCommaIndex = cleanedInput.LastIndexOf(',');
            var lastDotIndex = cleanedInput.LastIndexOf('.');

            if (lastCommaIndex > lastDotIndex)
            {
                // Treat the last comma as the decimal separator
                var wholeNumberPart = cleanedInput.Substring(0, lastCommaIndex).Replace(".", "");
                var decimalPart = cleanedInput.Substring(lastCommaIndex + 1);
                cleanedInput = wholeNumberPart + "." + decimalPart;
            }
            else
            {
                // Treat the last dot as the decimal separator
                var wholeNumberPart = cleanedInput.Substring(0, lastDotIndex).Replace(",", "");
                var decimalPart = cleanedInput.Substring(lastDotIndex + 1);
                cleanedInput = wholeNumberPart + "." + decimalPart;
            }
        }
        else if (cleanedInput.Contains(','))
        {
            // If only a comma is present, treat it as the decimal separator (EU format)
            var parts = cleanedInput.Split(',');
            var wholeNumberPart = parts[0]; // Get Whole Amount before comma.
            var decimalPart = parts.Length > 1 ? parts[1] : "0"; // Handle case without decimal part
            cleanedInput = wholeNumberPart + "." + decimalPart;
        }
        else if (cleanedInput.Contains('.'))
        {
            // If only a dot is present, treat it as the decimal separator (US format)
            var parts = cleanedInput.Split('.');
            var wholeNumberPart = parts[0]; // Get Whole Amount before dot.
            var decimalPart = parts.Length > 1 ? parts[1] : "0"; // Handle case without decimal part
            cleanedInput = wholeNumberPart + "." + decimalPart;
        }

        // Parse the cleaned string to double
        return double.Parse(cleanedInput, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Tries to parse the supplied date (in EU or US style) and returns it
    ///     in the specified output format (default: <c>M/d/yyyy</c>).
    ///     If parsing fails, the original string is returned unchanged.
    /// </summary>
    /// <param name="dateString">The date string to parse.</param>
    /// <param name="outputFormat">The desired output format (e.g. <c>yyyy-MM-dd</c>). Defaults to <c>M/d/yyyy</c>.</param>
    public static string ParseDate(string dateString, string outputFormat = "M/d/yyyy")
    {
        // Define possible date formats
        string[] daymonthyear =
        [
            "d/M/yy",
            "d/M/yyyy",
            "d-M-yy",
            "d-M-yyyy",
            "dd/MM/yy",
            "dd/MM/yyyy",
            "dd-MM-yy",
            "dd-MM-yyyy"
        ];

        string[] monthdayyear =
        [
            "M/d/yy", // Allow single-digit month/day
            "M/d/yyyy",
            "M-d-yy",
            "M-d-yyyy",
            "MM/dd/yy",
            "MM/dd/yyyy",
            "MM-dd-yy",
            "MM-dd-yyyy"
        ];
        // Try to parse the date

        return Convert.ToString(DateTime.TryParseExact(dateString, daymonthyear,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate)
            ? parsedDate.ToString(outputFormat)
            : DateTime.ParseExact(dateString, monthdayyear,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None)
                .ToString(outputFormat));
    }

    /// <summary>
    ///     Normalises line-breaks and trims superfluous whitespace.
    /// </summary>
    public static string NormalizeString(string input)
    {
        // Convert to uppercase
        var upper = input.ToUpper();

        // Replace new line characters with a space
        upper = upper.Replace(Environment.NewLine, "")
            .Replace("\n", "")
            .Replace("\r", "");

        var replace = upper.Replace(" ", string.Empty);
        return replace.ToUpper(); // Return the normalized string
    }

    /// <summary>
    ///    Returns the current week number based on the ISO 8601 standard.
    /// </summary>
    public static int GetWeekNumber()
    {
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            DateTime.Now,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday
            );
    }
}