namespace AuthAPI.Models
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string AuthToken { get; set; }
    }
}
