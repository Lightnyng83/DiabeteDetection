using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotesService.Models
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)] // Force la représentation en chaîne pour le Guid
        public Guid PatientId { get; set; }

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}