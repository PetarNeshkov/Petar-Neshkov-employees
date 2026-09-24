using PairOfEmployees.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler()
    .UseHttpsRedirection();
    
app.MapControllers();

app.Run();
