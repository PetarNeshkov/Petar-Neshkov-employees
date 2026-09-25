using PairOfEmployees.Server.Contracts;
using PairOfEmployees.Server.Models;
using PairOfEmployees.Server.Services.Parsing;
using PairOfEmployees.Server.Validation;

namespace PairOfEmployees.Server.Business.EmployeePairs;

public class EmployeePairsBusinessService(
    CsvUploadValidator uploadValidator,
    AssignmentCsvParser parser) 
    : IEmployeePairsBusinessService
{
    public async Task<AnalysisResponse> AnalyzeAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        uploadValidator.ValidateAndThrow(file);

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        var assignments = parser.Parse(stream, cancellationToken);
        var pair = FindLongestWorkingPair(assignments, cancellationToken);

        return new AnalysisResponse(pair);
    }

    private static EmployeePairResult? FindLongestWorkingPair(
        IEnumerable<EmployeeAssignment> assignments,
        CancellationToken cancellationToken)
    {
        var overlaps = new Dictionary<(int First, int Second), List<ProjectOverlap>>();
        var assignmentsByProject = assignments.GroupBy(assignment => assignment.ProjectId);

        foreach (var project in assignmentsByProject)
        {
            var employees = project.GroupBy(assignment => assignment.EmployeeId)
                .OrderBy(employee => employee.Key)
                .Select(employee => new
                {
                    Id = employee.Key,
                    Ranges = MergeRanges(employee.Select(assignment => new DateRange(assignment.DateFrom, assignment.DateTo)))
                }).ToArray();

            for (var firstEmployeeIndex = 0; firstEmployeeIndex < employees.Length; firstEmployeeIndex++)
            {
                for (var secondEmployeeIndex = firstEmployeeIndex + 1; secondEmployeeIndex < employees.Length; secondEmployeeIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var days = CountOverlap(employees[firstEmployeeIndex].Ranges, employees[secondEmployeeIndex].Ranges);
                    if (days == 0)
                    {
                        continue;
                    }

                    var pair = (employees[firstEmployeeIndex].Id, employees[secondEmployeeIndex].Id);
                    if (!overlaps.TryGetValue(pair, out var projects))
                    {
                        projects = [];
                        overlaps.Add(pair, projects);
                    }

                    var projectOverlap = new ProjectOverlap(pair.Item1, pair.Item2, project.Key, days);
                    projects.Add(projectOverlap);
                }
            }
        }

        return overlaps.Select(pair => new EmployeePairResult(
                pair.Key.First, pair.Key.Second, pair.Value.Sum(project => project.DaysWorked))
            {
                Projects = [.. pair.Value.OrderBy(project => project.ProjectId)]
            })
            .OrderByDescending(pair => pair.TotalDays)
            .ThenBy(pair => pair.EmployeeId1)
            .ThenBy(pair => pair.EmployeeId2)
            .FirstOrDefault();
    }

    // Sorts and combines overlapping or consecutive date ranges to avoid counting days twice.
    private static List<DateRange> MergeRanges(IEnumerable<DateRange> ranges)
    {
        var merged = new List<DateRange>();
        var orderedRanges = ranges.OrderBy(range => range.Start)
                                                            .ThenBy(range => range.End);

        foreach (var range in orderedRanges)
        {
            if (merged.Count == 0 || range.Start.DayNumber > merged[^1].End.DayNumber + 1)
            {
                merged.Add(range);
            }
            else if (range.End > merged[^1].End)
            {
                merged[^1] = merged[^1] with { End = range.End };
            }
        }

        return merged;
    }

    // Counts shared days, including both endpoints, between two sorted lists of merged date ranges.
    private static long CountOverlap(IReadOnlyList<DateRange> firstDateRanges, IReadOnlyList<DateRange> secondDateRanges)
    {
        var left = 0;
        var right = 0;
        long total = 0;

        while (left < firstDateRanges.Count && right < secondDateRanges.Count)
        {
            var start = Math.Max(firstDateRanges[left].Start.DayNumber, secondDateRanges[right].Start.DayNumber);
            var end = Math.Min(firstDateRanges[left].End.DayNumber, secondDateRanges[right].End.DayNumber);
            if (start <= end)
            {
                total += end - start + 1;
            }

            if (firstDateRanges[left].End <= secondDateRanges[right].End)
            {
                left++;
            }
            else
            {
                right++;
            }
        }

        return total;
    }
}
