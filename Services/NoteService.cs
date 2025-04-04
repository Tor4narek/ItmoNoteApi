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

      

        // Получить все записи
        public async Task<List<Note>> GetNotesAsync()
        {
            return await _context.Notes.ToListAsync();
        }

        // Добавить новую запись
        public async Task AddNoteAsync(string title,string description,string text,string category, int userId)
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
                Username = user.Username
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
                .Where(n => n.Category.Contains(category)) // Или используйте другие условия для фильтрации
                .ToListAsync();
        }
    }
}