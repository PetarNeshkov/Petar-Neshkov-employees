using System.Text;
using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Exceptions;
using PairOfEmployees.Server.Models;
using PairOfEmployees.Server.Services.Parsing;

namespace PairOfEmployees.Server.Tests;

public sealed class AssignmentCsvParserTests
{
    private readonly AssignmentCsvParser parser = new(new AssignmentDateParser(), new FixedTimeProvider());
    private static MemoryStream Csv(string content) 
        => new(Encoding.UTF8.GetBytes(content));

    [Theory]
    [InlineData("")]
    [InlineData("empid,projectid,datefrom,dateto\n")]
    public void Parse_WithOrWithoutHeader_ReadsQuotedDatesAndNullEndDate(string header)
    {
        using var stream = Csv(header + "1,10,\"February 1, 2024\",null\n2,10,2024-02-02,2024-02-03");
        var assignments = parser.Parse(stream);
        Assert.Equal(
            [
                new EmployeeAssignment(1, 10, new DateOnly(2024, 2, 1),
                    new DateOnly(2024, 2, 29)),
                new EmployeeAssignment(2, 10, new DateOnly(2024, 2, 2),
                    new DateOnly(2024, 2, 3))
            ],
            assignments);
    }

    [Theory]
    [InlineData("1,10,2024-01-01", AppConstants.InvalidColumns)]
    [InlineData("0,10,2024-01-01,NULL", AppConstants.InvalidIds)]
    [InlineData("1,-1,2024-01-01,NULL", AppConstants.InvalidIds)]
    [InlineData("1.5,10,2024-01-01,NULL", AppConstants.InvalidIds)]
    [InlineData("1,10,invalid,NULL", AppConstants.InvalidDateFrom)]
    [InlineData("1,10,2024-01-01,invalid", AppConstants.InvalidDateTo)]
    [InlineData("1,10,2024-02-02,2024-02-01", AppConstants.ReversedDates)]
    [InlineData("1,10,\"2024-01-01,NULL", AppConstants.MalformedQuoting)]
    public void Parse_InvalidRow_ReportsRowNumberAndReason(string row, string reason)
    {
        using var stream = Csv("EmpID,ProjectID,DateFrom,DateTo\n" + row);
        var error = Assert.Throws<RequestValidationException>(() => parser.Parse(stream));
        
        Assert.Equal(new[] { AppConstants.ForRow(2, reason) }, error.Errors[AppConstants.FileField]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \n\n")]
    [InlineData("EmpID,ProjectID,DateFrom,DateTo\n")]
    public void Parse_NoAssignments_RejectsFile(string input)
    {
        using var stream = Csv(input);
        var error = Assert.Throws<RequestValidationException>(() => parser.Parse(stream));
        
        Assert.Contains(AppConstants.EmptyCsv, error.Errors[AppConstants.FileField]);
    }

    [Fact]
    public void Parse_MultipleInvalidRows_ReportsAllRowNumbers()
    {
        using var stream = Csv("0,10,2024-01-01,NULL\n1,10,invalid,NULL");
        var error = Assert.Throws<RequestValidationException>(() => parser.Parse(stream));
        
        Assert.Equal(
            new[] { AppConstants.ForRow(1, AppConstants.InvalidIds), AppConstants.ForRow(2, AppConstants.InvalidDateFrom) },
            error.Errors[AppConstants.FileField]);
    }

    [Fact]
    public void Parse_TooManyErrors_StopsAtErrorLimit()
    {
        using var stream = Csv(string.Join('\n', Enumerable.Repeat("invalid", AppConstants.MaxErrors + 1)));
        var error = Assert.Throws<RequestValidationException>(() => parser.Parse(stream));
        var messages = error.Errors[AppConstants.FileField];
        
        Assert.Equal(AppConstants.MaxErrors + 1, messages.Length);
        Assert.Equal(AppConstants.ErrorLimit(AppConstants.MaxErrors), messages[^1]);
    }

    [Theory]
    [InlineData(AppConstants.MaxRows, true)]
    [InlineData(AppConstants.MaxRows + 1, false)]
    public void Parse_RowLimit_EnforcesBoundary(int rowCount, bool accepted)
    {
        using var stream = Csv(string.Join('\n', Enumerable.Repeat("1,10,2024-01-01,NULL", rowCount)));
        if (accepted)
        {
            Assert.Equal(rowCount, parser.Parse(stream).Count);
        }
        else
        {
            var error = Assert.Throws<RequestValidationException>(() => parser.Parse(stream));
            Assert.Contains(AppConstants.RowLimit(AppConstants.MaxRows), error.Errors[AppConstants.FileField]);
        }
    }

    [Fact]
    public void Parse_CanceledToken_Throws()
    {
        using var stream = Csv("1,10,2024-01-01,NULL");
        Assert.Throws<OperationCanceledException>(() => parser.Parse(stream, new CancellationToken(true)));
    }
}
