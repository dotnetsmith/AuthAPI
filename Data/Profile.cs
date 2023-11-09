namespace AuthAPI.Data
{
    public sealed record Profile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}