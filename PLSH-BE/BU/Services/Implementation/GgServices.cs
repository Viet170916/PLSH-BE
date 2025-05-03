using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BU.Services.Interface;
using Microsoft.Extensions.Configuration;

namespace BU.Services.Implementation;

public class GgServices(HttpClient httpClient, IConfiguration config) : IGgServices
{
  public class GoogleSearchResponse
  {
    public List<Item> Items { get; set; } = [];

    public class Item
    {
      public string Link { get; set; }
    }
  }

  public async Task<List<string>> SearchImageUrlsAsync(string query)
  {
    var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "AIzaSyAHcIVuLdQBxrbf1pa-rQVcqBmFPY7j25E";
    const string cx = "54f2596c3153e4008";
    var url =
      $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={cx}&searchType=image&q={Uri.EscapeDataString(query)}";
    var response = await httpClient.GetFromJsonAsync<GoogleSearchResponse>(url);
    return response?.Items?.Select(i => i.Link).ToList() ?? [];
  }
}
