using PairOfEmployees.Server.Contracts;

namespace PairOfEmployees.Server.Business.EmployeePairs;

public interface IEmployeePairsBusinessService
{
    Task<AnalysisResponse> AnalyzeAsync(IFormFile? file, CancellationToken cancellationToken = default);
}
