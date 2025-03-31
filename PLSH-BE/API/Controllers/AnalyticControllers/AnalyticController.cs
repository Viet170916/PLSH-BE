using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AnalyticControllers;

[ApiController]
[Route("api/v1/analytic")]
public partial class AnalyticController(AppDbContext context) : Controller { }
