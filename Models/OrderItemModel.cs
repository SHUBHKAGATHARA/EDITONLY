using System;
using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class OrderItemModel
    {
        public int OrderItemId { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        public string? OrderItemName { get; set; }

        [Display(Name = "Order ID")]
        public int? OrderId { get; set; }

        [Display(Name = "Product ID")]
        public int? ProductId { get; set; }

        [Required]
        public int? Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal? UnitPrice { get; set; }

        // Optional: For displaying names
        public string? ProductName { get; set; }
        public string? OrderName { get; set; }
    }
}
