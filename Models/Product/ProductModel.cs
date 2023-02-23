using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace App.Models.Product;
[Table("Product")]
public class ProductModel
{
    [Key]
    public int ProductId {get;set;}
    [Required]
    [DisplayName("Ten san pham")]
    // [StringLength(50,ErrorMessage ="{0} co do dai toi da {1} ky tu")]
    public string Title {get;set;}

    [DisplayName("Mo ta")]
    public string Description {get;set;}

    [DisplayName("Url hien thi")]
    [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage ="chi dung cac ky tu chu va so")]
    // [StringLength(100,ErrorMessage ="{0} co do dai toi da {1} ky tu")]
    public string? Slug {get;set;}

    // [DataType(DataType.Text)]
    [Required]
    [DisplayName("Noi dung")]
    public string Content {get;set;}

    [DisplayName("Ngay xuat ban")]
    public bool Published {get;set;}


    [DisplayName("Id Tac gia")]
    public string? AuthorId {get;set;}

    [ForeignKey("AuthorId")]
    [DisplayName("Tac gia")]
    public AppUser? Author {get;set;}

    [DataType(DataType.DateTime)]
    [DisplayName("Ngay tao")]
    public DateTime DateCreate {get;set;}

    [DataType(DataType.DateTime)]
    [DisplayName("Ngay cap nhat")]
    public DateTime DateUpdate {get;set;}
    [Required]
    [Display(Name = "Gia")]
    [Range(0, int.MaxValue,ErrorMessage = "{0} phai trong khoang tu {1} den {2}")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price {get;set;}
    public List<ProductPCategory>? ProductPCategories {get;set;}
}