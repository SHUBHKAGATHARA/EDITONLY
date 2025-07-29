using System;
using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class ContactMessageModel
    {
        public int MessageId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Message { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }
    }
}
