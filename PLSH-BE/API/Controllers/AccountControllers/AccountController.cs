using AutoMapper;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.AccountControllers;

[ApiController]
[Route("api/v1/account")]
// [Authorize(Roles = "Admin")] // Chỉ Admin 
public partial class AccountController(AppDbContext context, IEmailService emailService, IMapper mapper) : Controller
{
  [HttpPost("login")] public async Task<IActionResult> AccountService() { return Ok(); }
}