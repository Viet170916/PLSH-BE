using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using BU.Services.Interface;

public class GenAIService(HttpClient httpClient, ILogger<GenAIService> logger) : IGenAIService
{
    private const string ApiKey = "AIzaSyC7CudHtjqfDCk3ObZQ0ml1wjPpyc38nTE";
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/text-bison-001:predict";

    public async Task<List<int>> GetBookRecommendations(string bookIds)
    {
        try
        {
            var url = $"{ApiUrl}?key={ApiKey}";

            var request = new
            {
                prompt = new { text = $"Recommend books based on the following Book IDs: {bookIds}" },
                temperature = 0.7
            };

            var response = await httpClient.PostAsJsonAsync(url, request);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GenAIResponse>(jsonResponse);
                if (result != null && result.Predictions != null)
                {
                    return result.Predictions.Select(p => int.TryParse(p, out int value) ? value : 0).Where(v => v != 0).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while calling GenAI API.");
        }
        return new List<int>();
    }
}

public class GenAIResponse
{
    public List<string> Predictions { get; set; }
}
