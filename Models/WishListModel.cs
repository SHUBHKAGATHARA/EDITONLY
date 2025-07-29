using System;
using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class WishListModel
    {
        public int WishListId { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        public string? WishListItemName { get; set; }

        [Display(Name = "User ID")]
        public int? UserId { get; set; }

        [Display(Name = "Product ID")]
        public int? ProductId { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        // Optional display fields
        public string? UserName { get; set; }
        public string? ProductName { get; set; }
    }


}
