using System.Collections.Generic;
using System.Threading.Tasks;

namespace BU.Services.Interface;

public interface IGgServices
{
  Task<List<string>> SearchImageUrlsAsync(string query);
}
