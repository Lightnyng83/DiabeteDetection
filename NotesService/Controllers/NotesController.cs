using Microsoft.AspNetCore.Mvc;
using NotesService.Models;
using NotesService.Services;
using Microsoft.AspNetCore.Authorization;

namespace NotesService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Healthy");
    }


    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class NotesController : ControllerBase
    {
        private readonly NoteService _noteService;

        public NotesController(NoteService noteService)
        {
            _noteService = noteService;
        }

        // GET /api/notes/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetNotesForPatient(Guid patientId)
        {
            Console.WriteLine($"[DEBUG] GetNotesForPatient appelé avec patientId = {patientId}");
            var notes = await _noteService.GetNotesByPatientIdAsync(patientId);
            return Ok(notes);
        }



        // POST /api/notes
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            if (note == null || note.PatientId == Guid.Empty)
                return BadRequest("Invalid note data.");

            var created = await _noteService.CreateNoteAsync(note);
            return CreatedAtAction(nameof(GetNote), new { id = created.Id }, created);
        }

        // GET /api/notes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(string id)
        {
            var note = await _noteService.GetNoteAsync(id);
            if (note == null) return NotFound();
            return Ok(note);
        }

        // PUT /api/notes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] Note updated)
        {
            if (id != updated.Id)
                return BadRequest("ID mismatch");

            var note = await _noteService.GetNoteAsync(id);
            if (note == null) return NotFound();

            updated.CreatedAt = note.CreatedAt; // Conserver la date initiale
            await _noteService.UpdateNoteAsync(id, updated);
            return NoContent();
        }

        // DELETE /api/notes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var note = await _noteService.GetNoteAsync(id);
            if (note == null) return NotFound();

            await _noteService.DeleteNoteAsync(id);
            return NoContent();
        }
    }
}
