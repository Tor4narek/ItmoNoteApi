using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Services;

public class AiService : IAiService
{
    private readonly string _authorizationToken;
    private readonly string _apiUrl;

    public AiService(IConfiguration configuration)
    {
        _authorizationToken = configuration["АuthorizationToken:Key"];
        _apiUrl = configuration["AiApi:Url"];
    }

    // Генерация текста через нейросеть
    public async Task<string> GenerateTextWithAIAsync(string text, string prompt)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", _authorizationToken);

            string cleanedText = Regex.Replace(text, @"[^a-zA-Zа-яА-Я0-9]", "");
            var requestBody = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"{cleanedText} {prompt}"
                    }
                },
                max_tokens = 50000,
                model = "accounts/fireworks/models/deepseek-v3-0324",
                stream = false
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<ResponseModel>(result);
                return responseJson.Choices[0].Message.Content.Trim();
            }
            else
            {
                throw new Exception($"Error generating text: {response.StatusCode}");
            }
        }
    }

    // Создание Markdown-файла из текста
    public async Task<string> CreateMarkdownFileAsync(string text, string title)
    {
        string safeTitle = Regex.Replace(title, "[^a-zA-Zа-яА-Я0-9]", "_");
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, safeTitle + ".md");
        await File.WriteAllTextAsync(filePath, text, Encoding.UTF8);
        return $"/files/{safeTitle}.md";
    }
}


// Модель ответа от API (предполагается, что она у вас уже есть)
public class ResponseModel
{
    public Choice[] Choices { get; set; }
}

public class Choice
{
    public Message Message { get; set; }
}

public class Message
{
    public string Content { get; set; }
}