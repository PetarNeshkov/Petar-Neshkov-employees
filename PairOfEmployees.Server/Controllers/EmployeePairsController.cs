using Microsoft.AspNetCore.Mvc;
using PairOfEmployees.Server.Contracts;
using PairOfEmployees.Server.Exceptions;
using PairOfEmployees.Server.Services;
using PairOfEmployees.Server.Services.Analysis;
using PairOfEmployees.Server.Services.Parsing;
using PairOfEmployees.Server.Validation;

namespace PairOfEmployees.Server.Controllers;

[ApiController]
[Route("api/employee-pairs")]
public sealed class EmployeePairsController(EmployeeAnalysisService analysisService) : ControllerBase
{
    public const int MaxFileBytes = 1_048_576;

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileBytes + 65_536)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileBytes + 65_536, MemoryBufferThreshold = MaxFileBytes + 65_536)]
    [ProducesResponseType<AnalysisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<AnalysisResponse> Analyze([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            ModelState.AddModelError("file", "Select a non-empty CSV file.");
        else if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError("file", "The selected file must have a .csv extension.");
        else if (file.Length > MaxFileBytes)
            ModelState.AddModelError("file", "The file must not exceed 1 MiB.");

        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            using var stream = file!.OpenReadStream();
            //var assignments = parser.Parse(stream, cancellationToken);
            //return Ok(new AnalysisResponse(analyzer.Analyze(assignments, cancellationToken)));
            return Ok();
        }
        catch (CsvValidationException exception)
        {
            foreach (var error in exception.Errors) ModelState.AddModelError("file", error);
            return ValidationProblem(ModelState);
        }
    }
}
