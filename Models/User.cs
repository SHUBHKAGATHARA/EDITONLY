using System.ComponentModel.DataAnnotations;

namespace IceCreame_MVC.Models
{
    public class UserModel
    {
        // Changed from int to string to match ASP.NET Identity
        public int UserId { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}