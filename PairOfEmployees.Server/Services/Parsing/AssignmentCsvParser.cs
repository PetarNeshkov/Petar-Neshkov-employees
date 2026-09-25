using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Exceptions;
using PairOfEmployees.Server.Models;

namespace PairOfEmployees.Server.Services.Parsing;

public sealed class AssignmentCsvParser(AssignmentDateParser dates, TimeProvider clock)
{
    private const int MaxRows = AppConstants.MaxRows;
    private const int MaxErrors = AppConstants.MaxErrors;
    private static readonly IReadOnlyList<string> Header = AppConstants.Header;

    public IReadOnlyList<EmployeeAssignment> Parse(Stream stream, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        var assignments = new List<EmployeeAssignment>();
        var errors = new List<string>();
        using var parser = new TextFieldParser(stream, new UTF8Encoding(false, true), true, true)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(AppConstants.Delimiter);
        var firstRecord = true;
        var recordCount = 0;

        try
        {
            while (!parser.EndOfData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                parser.PeekChars(1);
                var row = parser.LineNumber;
                string[] fields;
                try
                {
                    fields = parser.ReadFields() ?? [];
                }
                catch (MalformedLineException)
                {
                    errors.Add(AppConstants.ForRow(parser.ErrorLineNumber, AppConstants.MalformedQuoting));
                    break;
                }

                if (firstRecord && fields.SequenceEqual(Header, StringComparer.OrdinalIgnoreCase))
                {
                    firstRecord = false;
                    continue;
                }

                firstRecord = false;
                if (++recordCount > MaxRows)
                {
                    errors.Add(AppConstants.RowLimit(MaxRows));
                    break;
                }

                if (fields.Length != AppConstants.ColumnCount)
                {
                    errors.Add(AppConstants.ForRow(row, AppConstants.InvalidColumns));
                }
                else if (!TryParseId(fields[AppConstants.EmployeeIdColumn], out var employeeId) ||
                         !TryParseId(fields[AppConstants.ProjectIdColumn], out var projectId))
                {
                    errors.Add(AppConstants.ForRow(row, AppConstants.InvalidIds));
                }
                else if (!dates.TryParse(fields[AppConstants.DateFromColumn], out var from))
                {
                    errors.Add(AppConstants.ForRow(row, AppConstants.InvalidDateFrom));
                }
                else
                {
                    var to = today;
                    if (!fields[AppConstants.DateToColumn].Equals(AppConstants.NullDate, StringComparison.OrdinalIgnoreCase) 
                        && !dates.TryParse(fields[AppConstants.DateToColumn], out to))
                    {
                        errors.Add(AppConstants.ForRow(row, AppConstants.InvalidDateTo));
                    }
                    else if (from > to)
                    {
                        errors.Add(AppConstants.ForRow(row, AppConstants.ReversedDates));
                    }
                    else
                    {
                        var employeeAssignment = new EmployeeAssignment(employeeId, projectId, from, to);
                        assignments.Add(employeeAssignment);
                    }
                }

                if (errors.Count >= MaxErrors)
                {
                    errors.Add(AppConstants.ErrorLimit(MaxErrors));
                    break;
                }
            }
        }
        catch (DecoderFallbackException)
        {
            errors.Add(AppConstants.InvalidEncoding);
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(AppConstants.FileField, [.. errors]);
        }
        
        if (assignments.Count == 0)
        {
            throw new RequestValidationException(AppConstants.FileField, AppConstants.EmptyCsv);
        }

        return assignments;
    }

    private static bool TryParseId(string value, out int id)
        => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
}
