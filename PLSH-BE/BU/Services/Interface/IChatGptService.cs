using System.Threading.Tasks;

namespace BU.Services.Interface;

public interface IChatGptService
{
  Task<string> SendMessageAsync(string message);
}
