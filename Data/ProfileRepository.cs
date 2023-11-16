using Dapper;
using AuthAPI.Models;

namespace AuthAPI.Data
{
    public class ProfileRepository : IProfileRepository
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
                var profile = await connection
                    .QuerySingleOrDefaultAsync<Profile>(query, new
                    {
                        Username = Username
                    }
                );

                return profile!;
            }
        }

        public async Task Create(UserRequest user)
        {
            var command = "INSERT INTO Profiles VALUES(@Username, @PasswordHash, @RefreshToken, @RefreshTokenExpiration)";

            using (var connection = _context.CreateConnection())
            {
                await connection.
                    ExecuteAsync(command, new
                    {
                        Username = user.Username,
                        PasswordHash = user.Password,
                        RefreshToken = (string)null,
                        RefreshTokenExpiration = (DateTime?)null
                    }
                );
            }
        }

        public async Task UpdateRefeshToken(string RefreshToken, DateTime? RefreshTokenExpiration, int Id)
        {
            var command = "UPDATE Profiles SET RefreshToken = @RefreshToken, RefreshTokenExpiration = @RefreshTokenExpiration WHERE Id = @Id";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(command, new
                {
                    RefreshToken = RefreshToken,
                    RefreshTokenExpiration = RefreshTokenExpiration,
                    Id = Id
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

        public async Task<Profile> GetProfileByRefreshToken(string refreshToken)
        {
            var query = "SELECT * FROM Profiles WHERE RefreshToken = @refreshToken";

            using (var connection = _context.CreateConnection())
            {
                var profile = await connection.QuerySingleOrDefaultAsync<Profile>(query, new
                {
                    RefreshToken = @refreshToken
                });

                return profile!;
            }
        }

        public async Task UpdatePassword(PasswordReset passwordReset)
        {
            var query = "UPDATE Profiles SET PasswordHash = @NewPassword WHERE Username = @Username";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, new
                {
                    PasswordHash = passwordReset.NewPassword,
                    Username = passwordReset.Username
                });
            }
        }
    }
}