using AuthAPI.Data;

namespace AuthAPI.Authentication
{
    public interface IJwtProvider
    {
        string Generate(Profile profile, HttpContext context);
    }
}