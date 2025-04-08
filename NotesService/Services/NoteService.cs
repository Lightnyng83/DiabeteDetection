using MongoDB.Driver;
using NotesService.Models;
using NotesService.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace NotesService.Services
{
    public class NoteService
    {
        private readonly IMongoCollection<Note> _notesCollection;

        public NoteService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _notesCollection = database.GetCollection<Note>(settings.Value.NotesCollectionName);
        }

        public async Task<List<Note>> GetNotesByPatientIdAsync(Guid patientId)
            => await _notesCollection.Find(n => n.PatientId == patientId).ToListAsync();

        public async Task<Note> CreateNoteAsync(Note note)
        {
            note.CreatedAt = DateTime.UtcNow;
            await _notesCollection.InsertOneAsync(note);
            return note;
        }

        public async Task<Note?> GetNoteAsync(string id)
        {
            // Si l'ID n'est pas un ObjectId valide, retournez null
            if (!ObjectId.TryParse(id, out _))
                return null;

            return await _notesCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
        }


    public async Task UpdateNoteAsync(string id, Note updatedNote)
            => await _notesCollection.ReplaceOneAsync(n => n.Id == id, updatedNote);

        public async Task DeleteNoteAsync(string id)
            => await _notesCollection.DeleteOneAsync(n => n.Id == id);
    }
}