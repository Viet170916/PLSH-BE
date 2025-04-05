using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace API.Provider;

public class AppUserIdProvider : IUserIdProvider
{
  public string? GetUserId(HubConnectionContext connection)
  {
    var a = connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  }
}
