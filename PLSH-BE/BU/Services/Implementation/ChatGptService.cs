using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BU.Services.Interface;
using Newtonsoft.Json;

namespace BU.Services.Implementation;

public class ChatGptService(HttpClient httpClient) : IChatGptService
{
  public async Task<string> SendMessageAsync(string message)
  {
    var request = new HttpRequestMessage
    {
      Method = HttpMethod.Post,
      RequestUri = new Uri("https://chatgpt-42.p.rapidapi.com/chat"),
      Headers =
      {
        { "x-rapidapi-key", "74e138399emshcf5125d3076d4d7p193111jsn12a2be465ae9" },
        { "x-rapidapi-host", "chatgpt-42.p.rapidapi.com" },
      },
      Content = new StringContent(JsonConvert.SerializeObject(new
      {
        messages = new[] { new { role = "user", content = message } },
        system_prompt = "",
        temperature = 0.9,
        top_k = 5,
        top_p = 0.9,
        max_tokens = 256,
        web_access = false,
      }), Encoding.UTF8, "application/json")
    };
    using var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    var responseString = await response.Content.ReadAsStringAsync();
    var json = JsonConvert.DeserializeObject<RapidApiResponse>(responseString.Replace("```json", "")
                                                                             .Replace("```", ""));
    var term = json?.Result.Replace("```json", "")
                   .Replace("```", "").Replace("\n","") ?? "No response conten";
    return term;
  }

  public class RapidApiResponse
  {
    [JsonProperty("result")]
    public string Result { get; set; }

    [JsonProperty("status")]
    public bool Status { get; set; }
  }
}
