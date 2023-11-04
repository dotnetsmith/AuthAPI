using Dapper;
using AuthAPI.Conext;
using AuthAPI.Entities;
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;

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
                var profile = await connection.QuerySingleOrDefaultAsync<Profile>(query, new
                {
                    Username = Username
                });

                return profile!;
            }
        }

        public async Task Create(UserRequest user)
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
                
        public async Task<Profile> GetProfileById(int id)
        {
            var query = "SELECT * FROM Profiles WHERE Id = @id";

            using (var connection = _context.CreateConnection())
            {
                var profile = await connection.QuerySingleOrDefaultAsync<Profile>(query, new
                {
                    Id = @id
                });

                return profile!;
            }
        }
    }
}