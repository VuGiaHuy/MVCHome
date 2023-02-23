using System.ComponentModel;

namespace App.Models.Product;
public class CreateProductModel : ProductModel
{
    [DisplayName("chuyen muc")]
    public int[] PCategoryIds {get;set;}
}