using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BU.Services.Interface;
using Common.Library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Model.Entity;
using Newtonsoft.Json;

namespace API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAccountService accountService) : Controller
{
  [HttpGet()] public IActionResult Get() { return Ok(new { message = "OK" }); }

  [HttpPost("sign-in")] public async Task<IActionResult> SignIn([FromBody] GoogleLoginRequest request)
  {
    var googleUser = await GetGoogleUserInfo(request.GoogleToken);
    if (googleUser == null) { return Unauthorized("Invalid token."); }

    var user = await accountService.GetOrCreateUserAsync(googleUser.Email);
    if (user == null)
    {
      return NotFound(new { message = "User not found.", isAuthorized = false, account = googleUser, });
    }

    var token = GenerateJwt(user);
    return Ok(new
    {
      message = "Login successful.", isAuthenticated = true, account = user, accessToken = token,
    });
  }

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
      new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    };
    var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? string.Empty));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(issuer: "your-issuer",
      audience: "your-audience",
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