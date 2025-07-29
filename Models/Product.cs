using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace IceCreame_MVC.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }

        public int? BrandId { get; set; }
        public string? BrandName { get; set; }

        public List<SelectListItem>? BrandList { get; set; }
    }

}
