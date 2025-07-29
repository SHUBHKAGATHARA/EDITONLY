using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Optional: For displaying related product names in views
        public List<string>? ProductNames { get; set; }
    }
}
