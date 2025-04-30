using Models.Entities;
using Storage;

namespace Services;

public interface INoteService
{
    Task AddNoteAsync(string title,string description,string text,string category,int userId, bool isPublic);
    Task DeleteNoteAsync(int id);
    Task<List<Note>> GetPublicNotesAsync();
    Task<List<Note>> GetUserNotesAsync( int userId, bool isPublic);
    
    Task<List<Note>> GetNotesByCategoryAsync(string category);
    Task<Note> GetNoteByIdAsync(int id);
    Task UpdateNoteAsync(int id, string text, string category, string title, string description);
    Task<String> FormatNoteAsync(string text, string prompt);
    Task<List<Category>> GetCategoriesAsync();
    Task CreateCategoryAsync(string category);
    Task UpdateCategoryAsync(int id, string category);
    Task DeleteCategoryAsync(int id);
  


}