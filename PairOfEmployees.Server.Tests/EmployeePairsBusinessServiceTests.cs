using System.Text;
using Microsoft.AspNetCore.Http;
using PairOfEmployees.Server.Business.EmployeePairs;
using PairOfEmployees.Server.Contracts;
using PairOfEmployees.Server.Exceptions;
using PairOfEmployees.Server.Services.Parsing;
using PairOfEmployees.Server.Validation;

namespace PairOfEmployees.Server.Tests;

public sealed class EmployeePairsBusinessServiceTests
{
    private readonly EmployeePairsBusinessService service =
        new(new CsvUploadValidator(), new AssignmentCsvParser(new AssignmentDateParser(), new FixedTimeProvider()));

    [Fact]
    public async Task AnalyzeAsync_MultipleProjects_SelectsLargestTotalAndSortsProjects()
    {
        var response = await Analyze("""
            2,20,2024-01-01,2024-01-10
            1,20,2024-01-05,2024-01-15
            1,10,2024-02-01,2024-02-03
            2,10,2024-02-01,2024-02-03
            3,30,2024-01-01,2024-01-08
            4,30,2024-01-01,2024-01-08
            """);

        var pair = Assert.IsType<EmployeePairResult>(response.Pair);
        
        Assert.Equal((1, 2, 9L), (pair.EmployeeId1, pair.EmployeeId2, pair.TotalDays));
        Assert.Equal([new ProjectOverlap(1, 2, 10, 3), new ProjectOverlap(1, 2, 20, 6)], pair.Projects);
    }

    [Fact]
    public async Task AnalyzeAsync_DuplicateOverlappingAndAdjacentAssignments_CountsEachDayOnce()
    {
        var response = await Analyze("""
            1,10,2024-01-04,2024-01-07
            1,10,2024-01-01,2024-01-05
            1,10,2024-01-01,2024-01-05
            1,10,2024-01-08,2024-01-10
            2,10,2024-01-01,2024-01-10
            2,10,2024-01-03,2024-01-06
            """);

        Assert.Equal(10L, Assert.IsType<EmployeePairResult>(response.Pair).TotalDays);
    }

    [Fact]
    public async Task AnalyzeAsync_SeparateIntervals_DoesNotCountGaps()
    {
        var response = await Analyze("""
            1,10,2024-01-01,2024-01-03
            1,10,2024-01-07,2024-01-10
            2,10,2024-01-02,2024-01-08
            """);

        Assert.Equal(4L, Assert.IsType<EmployeePairResult>(response.Pair).TotalDays);
    }

    [Fact]
    public async Task AnalyzeAsync_SharedEndpoint_CountsOneDay()
    {
        var response = await Analyze("1,10,2024-01-01,2024-01-05\n2,10,2024-01-05,2024-01-10");
        
        Assert.Equal(1L, Assert.IsType<EmployeePairResult>(response.Pair).TotalDays);
    }

    [Theory]
    [InlineData("1,10,2024-01-01,2024-01-05\n2,10,2024-01-06,2024-01-10")]
    [InlineData("1,10,2024-01-01,2024-01-05\n2,20,2024-01-01,2024-01-05")]
    [InlineData("1,10,2024-01-01,2024-01-05\n1,10,2024-01-01,2024-01-05")]
    public async Task AnalyzeAsync_NoSharedDaysBetweenDistinctEmployees_ReturnsNoPair(string csv)
    {
        Assert.Null((await Analyze(csv)).Pair);
    }

    [Fact]
    public async Task AnalyzeAsync_TiedTotals_SelectsLowestEmployeeIds()
    {
        var response = await Analyze("""
            3,10,2024-01-01,2024-01-05
            2,10,2024-01-01,2024-01-05
            1,10,2024-01-01,2024-01-05
            """);
        var pair = Assert.IsType<EmployeePairResult>(response.Pair);
        
        Assert.Equal((1, 2), (pair.EmployeeId1, pair.EmployeeId2));
    }

    [Fact]
    public async Task AnalyzeAsync_NullEndDates_UsesCurrentDate()
    {
        var response = await Analyze("1,10,2024-02-27,NULL\n2,10,2024-02-28,NULL");
        
        Assert.Equal(2L, Assert.IsType<EmployeePairResult>(response.Pair).TotalDays);
    }

    [Fact]
    public async Task AnalyzeAsync_MissingFile_ThrowsValidationError()
    {
        await Assert.ThrowsAsync<RequestValidationException>(() => service.AnalyzeAsync(null));
    }

    [Fact]
    public async Task AnalyzeAsync_CanceledToken_ThrowsBeforeValidation()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AnalyzeAsync(null, new CancellationToken(true)));
    }

    private async Task<AnalysisResponse> Analyze(string csv)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var file = new FormFile(stream, 0, stream.Length, "file", "employees.csv");
        
        return await service.AnalyzeAsync(file);
    }
}
