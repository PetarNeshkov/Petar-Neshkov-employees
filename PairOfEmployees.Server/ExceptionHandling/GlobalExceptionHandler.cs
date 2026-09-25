using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Exceptions;

namespace PairOfEmployees.Server.ExceptionHandling;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is RequestValidationException validationException)
        {
            problem = CreateValidationProblem(validationException.Errors);
        }
        else if (exception is DecoderFallbackException)
        {
            problem = CreateValidationProblem([AppConstants.InvalidEncoding]);
        }
        else if (exception is BadHttpRequestException requestException)
        {
            problem = new ProblemDetails
            {
                Status = requestException.StatusCode,
                Title = AppConstants.InvalidRequestTitle
            };
        }
        else
        {
            logger.LogError(exception, AppConstants.UnexpectedErrorLog,
                httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = AppConstants.UnexpectedErrorTitle,
                Detail = AppConstants.UnexpectedErrorDetail
            };
        }

        problem.Instance = httpContext.Request.Path;
        problem.Extensions[AppConstants.TraceIdField] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        // Serialize the concrete type so validation errors are included in the JSON.
        await httpContext.Response.WriteAsJsonAsync((object)problem,
            options: null, contentType: AppConstants.ProblemJsonContentType, cancellationToken: cancellationToken);
        return true;
    }

    private static ValidationProblemDetails CreateValidationProblem(string[] errors) =>
        CreateValidationProblem(new Dictionary<string, string[]> { [AppConstants.FileField] = errors });

    private static ValidationProblemDetails CreateValidationProblem(IReadOnlyDictionary<string, string[]> errors) =>
        new(errors.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = AppConstants.ValidationTitle
        };
}
