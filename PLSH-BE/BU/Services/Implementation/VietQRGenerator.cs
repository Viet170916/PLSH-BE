using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class VietQRGenerator
{
    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public VietQRGenerator(string clientId, string apiKey)
    {
        _clientId = clientId;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("dedd36cb-151f-4ee5-8687-addd3163caae", _clientId);
        _httpClient.DefaultRequestHeaders.Add("ec6220c7-91d2-4d7e-adee-5e7ab090c15e", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
    }

    public async Task<string> GenerateQRCode(string accountNo, string accountName, string acqId, int amount, string addInfo, string template)
    {
        var requestBody = new
        {
            accountNo,
            accountName,
            acqId,
            amount,
            addInfo = "Phí gia hạn trả sách",
            template
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.vietqr.io/v2/generate", content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
            return result.data.qrDataURL.ToString();
        }
        else
        {
            throw new Exception($"Không thể tạo mã QR: {response.StatusCode}");
        }
    }
}