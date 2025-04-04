using Storage;

namespace Services;

public interface INoteService
{
    Task AddNoteAsync(string title,string description,string text,string category,int userId);
    Task DeleteNoteAsync(int id);
    Task<List<Note>> GetNotesAsync();
    Task<List<Note>> GetNotesByCategoryAsync(string category);
    Task<Note> GetNoteByIdAsync(int id);
    Task UpdateNoteAsync(int id, string text, string category, string title, string description);
    Task<String> FormatNoteAsync(string text, string prompt);



}