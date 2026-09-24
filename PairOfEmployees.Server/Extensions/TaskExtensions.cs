using Microsoft.AspNetCore.Mvc;

namespace PairOfEmployees.Server.Extensions;

public static class TaskExtensions
{
    public static async Task<OkObjectResult> ToOkResult<T>(this Task<T> task)
        => new(await task);
}