using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        [Required]
        [Display(Name = "Order Name")]
        public string? OrderName { get; set; }
        [Display(Name = "User ID")]
        public int? UserId { get; set; }
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime? OrderDate { get; set; }
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal? TotalAmount { get; set; }
        [Display(Name = "Status")]
        public string? Status { get; set; } // ✅ Added 'Status' property  
                                            // Optional: for display purposes  
        public string? UserName { get; set; }
    }
}
