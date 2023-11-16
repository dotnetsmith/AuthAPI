using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace AuthAPI.Data
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("AuthConnection")!;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task Init()
        {
            using var connection = CreateConnection();
            await _initUsers();

            async Task _initUsers()
            {
                var sql = """
                CREATE TABLE IF NOT EXISTS 
                Profiles (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Username varchar(50) not null,
                        PasswordHash varchar(200) not null,
                        RefreshToken varchar(40) not null,
                        RefreshTokenExpiration datetime not null
                    );
                """;
                await connection.ExecuteAsync(sql);
            }
        }
    }
}