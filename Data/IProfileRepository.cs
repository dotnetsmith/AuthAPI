using AuthAPI.Models;

namespace AuthAPI.Data
{
    public interface IProfileRepository
    {
        Task Create(UserRequest user);
        Task<Profile> GetProfile(string Username);
        Task<Profile> GetProfileById(int id);
        Task<Profile> GetProfileByRefreshToken(string refreshToken);
        Task UpdateRefeshToken(string RefreshToken, DateTime RefreshTokenExpiration, int Id);
    }
}