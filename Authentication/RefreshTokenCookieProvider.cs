using System.Security.Cryptography;

namespace AuthAPI.Authentication
{
    public sealed class RefreshTokenCookieProvider : IRefreshTokenCookieProvider
    {
        public (string, DateTime) Generate(HttpContext context)
        {
            var refreshToken = Guid.NewGuid().ToString();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshTokenExpiration
            });

            return (refreshToken, refreshTokenExpiration);
        }
    }
}
