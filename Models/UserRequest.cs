using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models
{
    public class UserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}