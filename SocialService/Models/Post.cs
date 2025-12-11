namespace SocialService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public int UserId { get; set; }
    public int FitnessClassId { get; set; }
    public int WorkoutId { get; set; }
    public DateTime PostDate { get; set; }
    public string PostTitle { get; set; }
    public string PostContent { get; set; }
    
    public List<Comment> Comments { get; set; } = new();
}