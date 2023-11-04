using AuthAPI.Entities;

namespace AuthAPI.Abastractions
{
    public interface IJwtProvider
    {
        string Generate(Profile profile);
    }
}
