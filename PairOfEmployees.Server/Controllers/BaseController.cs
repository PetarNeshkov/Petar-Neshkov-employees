using Microsoft.AspNetCore.Mvc;
using PairOfEmployees.Server.Constants;

namespace PairOfEmployees.Server.Controllers;

[ApiController]
[Route(AppConstants.ControllerRoute)]
[Produces(AppConstants.JsonContentType)]
public class BaseApiController : ControllerBase
{
}