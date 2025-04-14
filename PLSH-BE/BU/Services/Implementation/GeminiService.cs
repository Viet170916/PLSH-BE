using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BU.Models.DTO.Book;
using BU.Services.Interface;
using Microsoft.Extensions.Options;
using Model.Entity.book;

namespace BU.Services.Implementation;

public class GeminiService(HttpClient httpClient)
  : IGeminiService
{
  private readonly string _apiKey =
    Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "AIzaSyAHcIVuLdQBxrbf1pa-rQVcqBmFPY7j25E";

  public async Task<BookNewDto?> GetBookFromGeminiPromptAsync(BookNewDto input)
  {
    var prompt =
      $"<Tên sách: {input.Title}, phiên bản: {input.Version ?? "(hãy cố tìm cho tôi thông tin chính xác)"}>. " +
      "Trả ra cho tôi kết quả đúng với mẫu json đây, lấy càng chi tiết càng tốt, id category phải bằng 0, description lấy tối thiểu 300 từ, không để thừa dấu phẩy ở thuộc tính cuối cùng của object hoặc mảng, nên có thông số vật lý, cần có phiên bản, authors bắt buộc phải có ít nhất 1 author, trường fullName của author phải là tên của author đó, không được để bất kì nội dung nào không liên quan ví dụ như 'nhiều tác giả', id sách lấy ngẫu nhiên 1 số 9 ký tự số và phải là số ngẫu nhiên khó đoán khó trùng lặp lại, còn các id của thuộc tính khác để null, quantity để là 0, ngôn ngữ thì để dạng kiểu mã ví dụ en hay vi ...,  thumbnail không có url thực tế thì để null, tất cả thông tin cần phải chính xác, nên có cả các số isbn và isbn bắt buộc phải chính xác, nếu không tìm được thông tin chính xác về isbn nào thì hãy để isbn đó null, không để thừa dấu phẩy ở thuộc tính cuối cùng của object hoặc mảng, đây là 1 câu prompt cho ứng dụng web, không trả lời các từ không cần thiết, chỉ cần trả lời mỗi kết quả cuối cùng. type Book = {\n\nid: number;\n\ntitle: string;\n\ndescription: string;\n\nlanguage: string;\n\npageCount: number;\n\nisbnNumber10: string;\n\nisbnNumber13: string;\n\notherIdentifier: string | null;\n\npublishDate: string;\n\npublisher: string;\n\nversion: string;\n\nquantity: number;\n\ncategoryId: number;\n\ncategory: {\n\nid: number;\n\nname: string;\n\ndescription: string | null;\n\n};\n\nnewCategory: {\n\nid: number | null;\n\nname: string;\n\ndescription: string | null;\n\nchosen: boolean;\n\n};\n\nauthors: {\n\nid: number;\n\nfullName: string;\n\navatarUrl: string | null;\n\ndescription: string | null;\n\nsummaryDescription: string | null;\n\nbirthYear: string | null;\n\ndeathYear: string | null;\n\nauthorImageResource: string | null;\n\nresource: any | null;\n\n}[];\n\navailability: any[]; // hoặc có thể định nghĩa rõ hơn nếu bạn biết cấu trúc phần tử\n\navailabilities: any[];\n\ncoverImageResource: {\n\ntype: string;\n\nlocalUrl: string;\n\n};\n\nthumbnail: string;\n\n};";
    var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
    var httpRequest = new HttpRequestMessage
    {
      Method = HttpMethod.Post,
      RequestUri =
        new Uri(
          $"https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash-001:generateContent?key={_apiKey}"),
      Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
    };
    var response = await httpClient.SendAsync(httpRequest);
    if (!response.IsSuccessStatusCode) { return null; }

    var responseContent = await response.Content.ReadAsStringAsync();
    using var document = JsonDocument.Parse(responseContent);
    var rawText = document
                  .RootElement
                  .GetProperty("candidates")[0]
                  .GetProperty("content")
                  .GetProperty("parts")[0]
                  .GetProperty("text")
                  .GetString()
                  ?.Replace("```json", "")
                  .Replace("```", "")
                  .Replace("\n", "");
    var parsedBook =
      JsonSerializer.Deserialize<BookNewDto>(rawText!,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return parsedBook;
  }

  public async Task<string> GetFromGeminiPromptAsync(string prompt)
  {
    var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
    var httpRequest = new HttpRequestMessage
    {
      Method = HttpMethod.Post,
      RequestUri =
        new Uri(
          $"https://generativelanguage.googleapis.com/v1/models/gemini-2.0-flash-001:generateContent?key={_apiKey}"),
      Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
    };
    var response = await httpClient.SendAsync(httpRequest);
    if (!response.IsSuccessStatusCode) { return null; }

    var responseContent = await response.Content.ReadAsStringAsync();
    using var document = JsonDocument.Parse(responseContent);
    var rawText = document
                  .RootElement
                  .GetProperty("candidates")[0]
                  .GetProperty("content")
                  .GetProperty("parts")[0]
                  .GetProperty("text")
                  .GetString()
                  ?.Replace("```json", "")
                  .Replace("```vi", "")
                  .Replace("```", "")
                  .Replace("\n", "");

    return rawText;
  }

  public class GeminiOptions
  {
    public string ApiKey { get; set; } = null!;
  }
}
