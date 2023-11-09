namespace AuthAPI.Models
{
    public sealed record UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }
}
