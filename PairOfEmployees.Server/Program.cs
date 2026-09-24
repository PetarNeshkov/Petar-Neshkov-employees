using PairOfEmployees.Server.Parsing;
using PairOfEmployees.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
AddEmployeeAnalysis(builder);

var app = builder.Build();

app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();

void AddEmployeeAnalysis(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddSingleton(TimeProvider.System);
    webApplicationBuilder.Services.AddSingleton<AssignmentDateParser>();
    webApplicationBuilder.Services.AddScoped<AssignmentCsvParser>();
    webApplicationBuilder.Services.AddScoped<EmployeePairAnalyzer>();
}

// Exposes the entry point to the API integration test host.
public partial class Program;
