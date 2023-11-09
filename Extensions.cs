using AuthAPI.Data;
using AuthAPI.Models;

namespace AuthAPI
{
    public static class Extensions
    {
        public static UserDto ToDto(this Profile profile)
        {
            return new UserDto
            {
                Id = profile.Id,
                Username = profile.Username
            };
        }
    }
}
