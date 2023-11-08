namespace AuthAPI.Models
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
    }
}
