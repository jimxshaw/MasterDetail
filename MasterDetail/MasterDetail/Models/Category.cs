using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TreeUtility;

namespace MasterDetail.Models
{
    public class Category : ITreeNode<Category>
    {
        public int Id { get; set; }

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        [Required(ErrorMessage = "You must enter a category name.")]
        [StringLength(20, ErrorMessage = "Category names must be 20 characters or shorter.")]
        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        public virtual List<InventoryItem> InventoryItems { get; set; }

        public virtual Category Parent { get; set; }

        public IList<Category> Children { get; set; }
    }
}