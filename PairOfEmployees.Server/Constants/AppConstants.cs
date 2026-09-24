using System.Globalization;

namespace PairOfEmployees.Server.Constants;

public static class AppConstants
{
    // API
    public const string ControllerRoute = "api/[controller]/[action]";
    public const string JsonContentType = "application/json";
    public const string ProblemJsonContentType = "application/problem+json";
    public const string MultipartContentType = "multipart/form-data";
    public const string FileField = "file";
    public const string TraceIdField = "traceId";

    // Upload and CSV rules
    public const int MaxFileBytes = 1_048_576;
    public const int MultipartOverheadBytes = 65_536;
    public const int MaxRequestBytes = MaxFileBytes + MultipartOverheadBytes;
    public const string Extension = ".csv";
    public const string Delimiter = ",";
    public const string NullDate = "NULL";
    public const int MaxRows = 10_000;
    public const int MaxErrors = 20;
    public const int ColumnCount = 4;
    public const int EmployeeIdColumn = 0;
    public const int ProjectIdColumn = 1;
    public const int DateFromColumn = 2;
    public const int DateToColumn = 3;
    public static readonly IReadOnlyList<string> Header =
        Array.AsReadOnly(["EmpID", "ProjectID", "DateFrom", "DateTo"]);

    public static readonly CultureInfo BulgarianCulture = CultureInfo.GetCultureInfo("bg-BG");
    public static readonly IReadOnlyList<string> DateFormats = Array.AsReadOnly([
        "dd.MM.yyyy", "d.M.yyyy", "d/M/yyyy", "d-M-yyyy",
        "d MMMM yyyy", "d MMM yyyy", "d-MMMM-yyyy", "d-MMM-yyyy",
        "d.M.yyyy 'г.'", "d MMMM yyyy 'г.'", "d MMM yyyy 'г.'"
    ]);
    public static readonly IReadOnlyList<IReadOnlyList<string>> Groups =
        Array.AsReadOnly<IReadOnlyList<string>>([
            Array.AsReadOnly(["d.M.yyyy", "d/M/yyyy", "d-M-yyyy"]),
            Array.AsReadOnly(["yyyy-M-d", "yyyy/M/d", "yyyy.M.d"]),
            Array.AsReadOnly([
                "d MMM yyyy", "d MMMM yyyy", "d-MMM-yyyy", "d-MMMM-yyyy",
                "MMM d yyyy", "MMMM d yyyy", "MMM d, yyyy", "MMMM d, yyyy"
            ]),
            Array.AsReadOnly(["M-d-yyyy", "M/d/yyyy", "M.d.yyyy"])
        ]);

    // Validation messages
    public const string InvalidRequest = "The request contains invalid data.";
    public const string FileRequired = "Select a non-empty CSV file.";
    public const string CsvExtensionRequired = "The selected file must have a .csv extension.";
    public const string FileTooLarge = "The file must not exceed 1 MiB.";
    public const string InvalidEncoding = "The file contains invalid text encoding. Save it as UTF-8 CSV.";
    public const string EmptyCsv = "The CSV file must contain at least one assignment row.";
    public const string MalformedQuoting = "malformed CSV quoting.";
    public const string InvalidColumns = "expected 4 columns (EmpID, ProjectID, DateFrom, DateTo).";
    public const string InvalidIds = "employee and project IDs must be positive whole numbers.";
    public const string InvalidDateFrom = "DateFrom is invalid. Use a supported date such as 31.01.2024.";
    public const string InvalidDateTo = "DateTo must be a supported date or NULL.";
    public const string ReversedDates = "DateFrom must not be after DateTo.";
    private const string RowMessage = "Row {0}: {1}";
    private const string RowLimitMessage = "The file may contain at most {0:N0} assignment rows.";
    private const string ErrorLimitMessage = "Only the first {0} errors are shown. Correct them and upload again.";

    // Error responses and logging
    public const string InvalidRequestTitle = "The HTTP request could not be processed.";
    public const string ValidationTitle = "One or more validation errors occurred.";
    public const string UnexpectedErrorTitle = "An unexpected server error occurred.";
    public const string UnexpectedErrorDetail = "Please try again later.";
    public const string UnexpectedErrorLog = "Unhandled error for {Method} {Path}. Trace ID: {TraceId}";

    public static string ForRow(long row, string message) =>
        string.Format(CultureInfo.InvariantCulture, RowMessage, row, message);

    public static string RowLimit(int maximum) =>
        string.Format(CultureInfo.InvariantCulture, RowLimitMessage, maximum);

    public static string ErrorLimit(int maximum) =>
        string.Format(CultureInfo.InvariantCulture, ErrorLimitMessage, maximum);
}
