using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItmoNoteAPI.Controllers
{
    [Route("api/notes")]
    [ApiController]
     // Требует аутентификации через JWT
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        // Получить все записи
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            var notes = await _noteService.GetNotesAsync();
            return Ok(notes);
        }

        // Добавить новую запись
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateNote(string title, string description, string category, int userId, [FromBody] string text)
        {
            await _noteService.AddNoteAsync(title, description, text, category, userId);
            return CreatedAtAction(nameof(GetNotes), new { }, null);
        }

        // Получить запись по Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNoteById(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        // Обновить запись
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateNote(int id, string title, string description, string category, [FromBody] string text)
        {
            var existingNote = await _noteService.GetNoteByIdAsync(id);
            if (existingNote == null)
            {
                return NotFound();
            }

            await _noteService.UpdateNoteAsync(id, text, category, title, description);
            return NoContent();
        }

        // Удалить запись по Id
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNote(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            await _noteService.DeleteNoteAsync(id);
            return NoContent();
        }

        // Получить записи по категории
        
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesByCategory([FromQuery] string category)
        {
            var notes = await _noteService.GetNotesByCategoryAsync(category);
            return Ok(notes);
        }

        // Форматировать текст заметки
        [HttpPost("format")]
        public async Task<ActionResult<string>> FormatNote([FromBody] FormatNoteRequest request)
        {
            var formattedText = await _noteService.FormatNoteAsync(request.Text, request.Prompt);
            return Ok(formattedText);
        }
    }

    public class FormatNoteRequest
    {
        public string Text { get; set; }
        public string Prompt { get; set; }
    }
}
