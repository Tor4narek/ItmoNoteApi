using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Storage;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;

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

        // Получить все публичные записи
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetPublicNotes()
        {
            var notes = await _noteService.GetPublicNotesAsync();
            return Ok(notes);
        }
        // Получить все приватные записи пользователя
        [Authorize]
        [HttpGet("private")]
        public async Task<ActionResult<IEnumerable<Note>>> GetPrivateNotes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Не удалось получить идентификатор пользователя из токена.");
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest("Неверный формат идентификатора пользователя");
            }

            var notes = await _noteService.GetUserNotesAsync(userId, false);
            return Ok(notes);
        }

        
        // Добавить новую запись
        // [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateNote(string title, string description, string category, int userId, bool isPublic, [FromBody] string text)
        {
            await _noteService.AddNoteAsync(title, description, text, category, userId, isPublic);
            return Ok();
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
        // [Authorize]
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
        // [Authorize]
        [HttpPost("format")]
        public async Task<ActionResult<string>> FormatNote([FromBody] FormatNoteRequest request)
        {
            var formattedText = await _noteService.FormatNoteAsync(request.Text, request.Prompt);
            return Ok(formattedText);
        }
        [HttpGet("categories")]
         public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
               {
                   var categories = await _noteService.GetCategoriesAsync();
                   return Ok(categories);
               }
        [HttpPost("category")]
         public async Task<ActionResult> CreateCategory(string name)
               {
                    await _noteService.CreateCategoryAsync(name);
                    return CreatedAtAction(nameof(GetCategories), new { }, null);
               }
    }

    public class FormatNoteRequest
    {
        public string Text { get; set; }
        public string Prompt { get; set; }
    }
}
