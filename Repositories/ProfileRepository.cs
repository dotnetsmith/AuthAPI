using Dapper;
using AuthAPI.Conext;
using AuthAPI.Entities;
using AuthAPI.Models;

namespace AuthAPI.Repositories
{
    public class ProfileRepository
    {
        private readonly DapperContext _context;

        public ProfileRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Profile> GetProfile(string Username)
        {
            var query = "SELECT * FROM Profiles WHERE Username = @Username";

            using (var connection = _context.CreateConnection())
            {
                var profiles = await connection.QuerySingleOrDefaultAsync<Profile>(query, new
                {
                    Username = Username
                });

                return profiles!;
            }
        }

        public async Task Create(User user)
        {
            var command = "INSERT INTO Profiles VALUES(@Username, @Password)";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(command, new
                {
                    Username = user.Username,
                    Password = user.Password
                });
            }
        }

    }
}