using System.ComponentModel.DataAnnotations;

namespace SocialService.Models;

public class Comment
{
    public int commentId  { get; set; }
    public int authorId{ get; set; }
    public DateTime commentDate { get; set; }
    public string commentText { get; set; }
}