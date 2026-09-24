using Microsoft.AspNetCore.Mvc;
using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Business.EmployeePairs;
using PairOfEmployees.Server.Contracts;
using PairOfEmployees.Server.Extensions;

namespace PairOfEmployees.Server.Controllers;

public class EmployeePairsController(IEmployeePairsBusinessService employeePairsBusinessService) : BaseApiController
{
    [HttpPost]
    [Consumes(AppConstants.MultipartContentType)]
    [RequestSizeLimit(AppConstants.MaxRequestBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = AppConstants.MaxRequestBytes,
        MemoryBufferThreshold = AppConstants.MaxRequestBytes)]
    [ProducesResponseType<AnalysisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze([FromForm(Name = AppConstants.FileField)] IFormFile? file, CancellationToken cancellationToken)
        => await employeePairsBusinessService.AnalyzeAsync(file, cancellationToken)
            .ToOkResult();
}
