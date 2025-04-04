using Storage;

namespace Services;

public interface IAiService
{
    Task<string> GenerateTextWithAIAsync(string text, string prompt);
    Task<string> CreateMarkdownFileAsync(string text, string title);
}