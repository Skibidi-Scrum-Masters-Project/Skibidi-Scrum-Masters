namespace SocialService.Models;

public class Post
{
    public int userId { get; set; }
    public int postId { get; set; }
    public int fitnessClassId { get; set; }
    public int workoutId { get; set; }
    public DateTime postDate { get; set; }
    public string postTitle { get; set; }
    public string postContent { get; set; }
    
    public List<string> comments { get; set; }
    
}