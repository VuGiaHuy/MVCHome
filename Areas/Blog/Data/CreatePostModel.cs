using System.ComponentModel;

namespace App.Models.Blog;
public class CreatePostModel : Post
{
    [DisplayName("chuyen muc")]
    public int[] CategoryIds {get;set;}
}