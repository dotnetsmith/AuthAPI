using AuthAPI.Data;

namespace AuthAPI.Authentication
{
    public interface IJwtHeaderProvider
    {
        string Generate(Profile profile, HttpContext context);
    }
}