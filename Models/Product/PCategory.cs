using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product {
  public class PCategory
  {
      [Key]
      public int Id { get; set; }

      // Category cha (FKey)
      [Display(Name = "Danh mục cha")]
      public int? ParentCategoryId { get; set; }

      // Tiều đề Category
      [Required(ErrorMessage = "Phải có tên danh mục")]
      [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
      [Display(Name = "Tên danh mục")]
      public string Title { get; set; }

      // Nội dung, thông tin chi tiết về Category
      [DataType(DataType.Text)]
      [Display(Name = "Nội dung danh mục")]
      public string Content { set; get; }

      //chuỗi Url
      [Required(ErrorMessage = "Phải tạo url")]
      [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
      [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
      [Display(Name = "Url hiện thị")]
      public string Slug { set; get; }

      // Các Category con
      public ICollection<PCategory>? ChildrenCategory { get; set; }

      [ForeignKey("ParentCategoryId")]
      [Display(Name = "Danh mục cha")]


      public PCategory? ParentCategory { set; get; }
      public void ChildCategoryIDs(List<int> list,ICollection<PCategory> childCate)
      {
          if(childCate == null)
            return;     
          if(childCate?.Count > 0)
          {
              foreach(PCategory cate in childCate)
              {                       
                  list.Add(cate.Id);
                  ChildCategoryIDs(list,cate.ChildrenCategory);                                                  
              }
          }
      }
      public List<PCategory> ListParent()
      {
        List<PCategory> list = new List<PCategory>();
        var parent = this.ParentCategory;
        while(parent != null)
        {
          list.Add(parent);
          parent = parent.ParentCategory;
        }
        list.Reverse();
        return list;
      }
  }
} 