using System.ComponentModel.DataAnnotations.Schema;
namespace App.Models.Product;
[Table("ProductPCategory")]
public class ProductPCategory
{
    public int ProductId {get;set;}
    public int PCategoryId {get;set;}

    [ForeignKey("ProductId")]
    public ProductModel Post{get;set;}
    
    [ForeignKey("PCategoryId")]
    public PCategory Category {get;set;}
}