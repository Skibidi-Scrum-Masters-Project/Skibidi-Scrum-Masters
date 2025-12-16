using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialService.Models;

public class Comment
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string AuthorId { get; set; }
    public string AuthorName { get; set; }
    public DateTime CommentDate { get; set; }
    public string CommentText { get; set; }
}