using System;
using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class CartModel
    {
        public int CartId { get; set; }

        [Display(Name = "Item Name")]
        public string? CartItemName { get; set; }

        [Display(Name = "User ID")]
        public int? UserId { get; set; }

        [Display(Name = "Product ID")]
        public int? ProductId { get; set; }

        public int? Quantity { get; set; }

        [Display(Name = "Created At")]
        [DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }

        // Navigation properties (for view purposes if needed)
        public string? ProductName { get; set; }
        public string? UserName { get; set; }
    }
}
