using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BU.Services.Interface;
using Common.Enums;
using Common.Helper;
using Common.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model.Entity;
using Model.ResponseModel;
using Newtonsoft.Json;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAccountService accountService, ILogger<AuthController> logger) : Controller
{
  [HttpGet("check-token")] public IActionResult CheckToken()
  {
    var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    var id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "-1");
    var fullname = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    return Ok(new OkResponse { Data = new { Id = id, Fullname = fullname, Email = email }, });
  }

  [AllowAnonymous] [HttpPost("sign-in")] public async Task<IActionResult> SignIn([FromBody] GoogleLoginRequest request)
  {
    try
    {
      logger.LogInformation($">>>>>>>Request: /api/v1/auth/sign-in...............");
      var googleUser = await GetGoogleUserInfo(request.GoogleToken);
      if (googleUser == null)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.UNAUTHORIZED.GetDescription(),
          StatusCode = HttpStatus.UNAUTHORIZED,
          Message = "Unauthorized access.",
        });
      }

      var user = await accountService.GetOrCreateUserAsync(googleUser.Email);
      if (user is null)
      {
        return Ok(new ErrorResponse
        {
          Status = HttpStatus.NOT_FOUND.GetDescription(),
          StatusCode = HttpStatus.NOT_FOUND,
          Message = "User not found.",
          Data = new { IsAuthorized = false, account = googleUser.RemoveNullProperties(), },
        });
      }

      var token = GenerateJwt(user);
      return Ok(new OkResponse
      {
        Status = HttpStatus.OK.GetDescription(),
        StatusCode = HttpStatus.OK,
        Message = "Login successful.",
        Data = new { IsAuthenticated = true, Account = user.RemoveNullProperties(), AccessToken = token, },
      });
    }
    catch (HttpRequestException e)
    {
      logger.LogError(e, ">>>>>>>Request: /api/v1/auth/sign-in...............An error occured");
      return Ok(new ErrorResponse
      {
        Status = HttpStatus.UNAUTHORIZED.GetDescription(),
        StatusCode = HttpStatus.UNAUTHORIZED,
        Message = "Unauthorized access.",
      });
    }
  }

  // [Authorize(Policy = "Bearer")] 
  private async Task<GoogleUserInfo> GetGoogleUserInfo(string googleToken)
  {
    var client = new HttpClient();
    var response =
      await client.GetStringAsync($"{SourceConstants.GET_USER_INFO}?access_token={googleToken}");
    return JsonConvert.DeserializeObject<GoogleUserInfo>(response);
  }

  private string GenerateJwt(Account user)
  {
    var claims = new[]
    {
      new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
    };
    var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(Constants.JWT_SECRET) ?? string.Empty));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(issuer: Constants.Issuer,
      audience: Constants.Audience,
      claims: claims,
      expires: DateTime.Now.AddDays(1),
      signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}

public class GoogleLoginRequest
{
  public string GoogleToken { get; set; }
}

public class GoogleUserInfo
{
  public string Sub { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public string Picture { get; set; }
}