using PairOfEmployees.Server.Business.EmployeePairs;
using PairOfEmployees.Server.ExceptionHandling;
using PairOfEmployees.Server.Services.Analysis;
using PairOfEmployees.Server.Services.Parsing;
using PairOfEmployees.Server.Validation;

namespace PairOfEmployees.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AssignmentDateParser>();
        services.AddSingleton<CsvUploadValidator>();
        services.AddScoped<AssignmentCsvParser>();
        services.AddScoped<EmployeePairAnalyzer>();
        services.AddScoped<IEmployeePairsBusinessService, EmployeePairsBusinessService>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}
