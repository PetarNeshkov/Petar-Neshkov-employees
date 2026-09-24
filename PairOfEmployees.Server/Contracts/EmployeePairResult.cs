namespace PairOfEmployees.Server.Contracts;

public sealed record EmployeePairResult(
    int EmployeeId1,
    int EmployeeId2,
    long TotalDays,
    IReadOnlyList<ProjectOverlap> Projects);
