using System.Security.Cryptography;

namespace AuthAPI.Authentication
{
    public sealed class RefreshTokenProvider : IRefreshTokenProvider
    {
        public (string, DateTime) Generate()
        {
            return (Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(7))!;
        }
    }
}
