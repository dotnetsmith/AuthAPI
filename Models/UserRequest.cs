using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models
{
    public sealed class UserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}