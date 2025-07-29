using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class BrandModel
    {
        public int BrandId { get; set; }

        [Display(Name = "Brand Name")]
        public string? BrandName { get; set; }

        public string? Description { get; set; }
    }
}