namespace AuthAPI.Authentication
{
    public interface IRefreshTokenCookieProvider
    {
        (string, DateTime) Generate(HttpContext context);
    }
}