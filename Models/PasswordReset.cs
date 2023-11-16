namespace AuthAPI.Models
{
    public sealed class PasswordReset
    {
        public required string Username { get; set; } = string.Empty;
        public required string OldPassword { get; set; } = string.Empty;
        public required string NewPassword { get; set; } = string.Empty;
    }
}
