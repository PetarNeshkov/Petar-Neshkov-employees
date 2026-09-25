namespace PairOfEmployees.Server.Contracts;

public sealed record ProjectOverlap(int EmployeeId1, int EmployeeId2, int ProjectId, long DaysWorked);
