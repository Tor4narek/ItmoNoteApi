using Microsoft.EntityFrameworkCore;
using Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public class NoteService : INoteService
    {
        private readonly ApplicationContext _context;
        private readonly IAiService _aiService;
        private readonly IUserService _userService;
        
        public NoteService(ApplicationContext context, IAiService aiService, IUserService userService)
        {
            _context = context;
            _aiService = aiService;
            _userService = userService;
        }
        
        // Получить публичные записи
        public async Task<List<Note>> GetPublicNotesAsync()
        {
            return await _context.Notes
                .Where(n => n.IsPublic.Equals(true)) // Или используйте другие условия для фильтрации
                .ToListAsync();
        }
        // Получить публичные записи
        public async Task<List<Note>> GetUserNotesAsync(int userId, bool isPublic)
        {
           var user =  await _userService.GetUserById(userId);
            return await _context.Notes
                .Where(n => n.IsPublic.Equals(false)) // Или используйте другие условия для фильтрации
                .Where(n => n.Username.Equals(user.Username))
                .ToListAsync();
        }

        // Добавить новую запись
        public async Task AddNoteAsync(string title,string description,string text,string category, int userId, bool isPublic)
        {
            var file = _aiService.CreateMarkdownFileAsync( text ,  title);
            var user = await _userService.GetUserById(userId);
            var note = new Note
            {
                Title = title,
                Description = description,
                File = await file,
                Category = category,
                Date = DateTime.UtcNow,
                Username = user.Username,
                IsPublic = isPublic
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task<String> FormatNoteAsync(string text, string prompt)
        {
            var newText = await _aiService.GenerateTextWithAIAsync(text, prompt);
            return newText;
        }

        public async Task UpdateNoteAsync(int id, string text, string category, string title, string description)
        {
            var file = await _aiService.CreateMarkdownFileAsync( text ,  title);
            var oldNote = await GetNoteByIdAsync(id);
            oldNote.Title = title;
            oldNote.Description = description;
            oldNote.Category = category;
            oldNote.Date = DateTime.UtcNow;
            oldNote.File = file;
            _context.Notes.Update(oldNote);
            await _context.SaveChangesAsync();
        }
        // Получить запись по Id
        public async Task<Note> GetNoteByIdAsync(int id)
        {
            return await _context.Notes.FindAsync(id);
        }

        public async Task DeleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }
        // Получить записи по заголовку (или тексту)
        public async Task<List<Note>> GetNotesByCategoryAsync(string category)
        {
            return await _context.Notes
                .Where(n => n.Category.Contains(category) )
                .Where( n => n.IsPublic.Equals(true))
                // Или используйте другие условия для фильтрации
                .ToListAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task CreateCategoryAsync(string category)
        {
            _context.Categories.Add(new Category {Name = category });
            await _context.SaveChangesAsync();
        }
        public async Task UpdateCategoryAsync(int id, string category)
        {
            var oldCategory = await GetCategoryById(id);
            oldCategory.Name = category;
            _context.Categories.Update(oldCategory);
            await _context.SaveChangesAsync();
        }
        private async Task<Category> GetCategoryById(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task DeleteCategoryAsync(int id)
        {
           var category = await GetCategoryById(id);
           if (category != null)
           {
               _context.Categories.Remove(category);
               await _context.SaveChangesAsync();
           }
           
        }
    }
}