namespace PairOfEmployees.Server.Models;

public sealed record EmployeeAssignment(int EmployeeId, int ProjectId, DateOnly DateFrom, DateOnly DateTo);
