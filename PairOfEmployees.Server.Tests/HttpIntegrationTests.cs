using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Contracts;
using PairOfEmployees.Server.Controllers;

namespace PairOfEmployees.Server.Tests;

[Trait("Category", "Integration")]
public sealed class HttpIntegrationTests(AnalysisApplication application) : IClassFixture<AnalysisApplication>, IDisposable
{
    private readonly HttpClient _client = application.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task Analyze_ValidMultipartUpload_ReturnsCamelCaseResult()
    {
        using var content = Upload(Encoding.UTF8.GetBytes(
            "EmpID,ProjectID,DateFrom,DateTo\n2,10,2024-02-27,NULL\n1,10,2024-02-28,NULL"));
        using var response = await _client.PostAsync("/api/EmployeePairs/Analyze", content);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var pair = json.RootElement.GetProperty("pair");
        
        Assert.Equal(1, pair.GetProperty("employeeId1").GetInt32());
        Assert.Equal(2, pair.GetProperty("employeeId2").GetInt32());
        Assert.Equal(2, pair.GetProperty("totalDays").GetInt64());
        
        var project = Assert.Single(pair.GetProperty("projects").EnumerateArray());
        Assert.Equal(10, project.GetProperty("projectId").GetInt32());
        Assert.Equal(2, project.GetProperty("daysWorked").GetInt64());
    }

    [Fact]
    public async Task Analyze_NoOverlap_ReturnsSuccessfulNullPair()
    {
        using var content = Upload(Encoding.UTF8.GetBytes("1,10,2024-01-01,NULL"));
        using var response = await _client.PostAsync("/api/EmployeePairs/Analyze", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(body);
        Assert.Null(body.Pair);
    }

    [Fact]
    public async Task Analyze_InvalidRow_ReturnsValidationProblem()
    {
        using var content = Upload("0,10,2024-01-01,NULL"u8.ToArray());
        using var response = await _client.PostAsync("/api/EmployeePairs/Analyze", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.Equal(AppConstants.ValidationTitle, root.GetProperty("title").GetString());
        Assert.Contains("Row 1: employee and project IDs must be positive whole numbers.",
            root.GetProperty("errors").GetProperty("file")
                .EnumerateArray().Select(value => value.GetString()));
    }

    public void Dispose() => _client.Dispose();

    private static MultipartFormDataContent Upload(byte[] bytes, string fileName = "employees.csv")
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(bytes), "file", fileName);
        
        return content;
    }
}

public sealed class AnalysisApplication : WebApplicationFactory<EmployeePairsController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider());
        });
    }
}
