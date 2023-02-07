using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Models.Blog;
public class Post
{
    [Key]
    public int PostId {get;set;}
    [Required]
    [DisplayName("Tieu de")]
    [StringLength(50,ErrorMessage ="{0} co do dai toi da {1} ky tu")]
    public string Title {get;set;}

    [DisplayName("Mo ta")]
    public string Description {get;set;}

    [DisplayName("Url hien thi")]
    [RegularExpression(@"^[a-z0-9]*$", ErrorMessage ="chi dung cac ky tu chu va so")]
    [StringLength(100,ErrorMessage ="{0} co do dai toi da {1} ky tu")]
    public string? Slug {get;set;}

    [DataType(DataType.Text)]
    [Required]
    [DisplayName("Noi dung")]
    public string Content {get;set;}

    [DisplayName("Ngay xuat ban")]
    public bool Published {get;set;}
    public List<PostCategory> PostCategories {get;set;}


    [Required]
    [DisplayName("Id Tac gia")]
    public string AuthorId {get;set;}

    [ForeignKey("AuthorId")]
    [DisplayName("Tac gia")]
    public AppUser Author {get;set;}

    [DataType(DataType.DateTime)]
    [DisplayName("Ngay tao")]
    public DateTime DateCreate {get;set;}

    [DataType(DataType.DateTime)]
    [DisplayName("Ngay cap nhat")]
    public DateTime DateUpdate {get;set;}
}