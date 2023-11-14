using AuthAPI.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuthAPI.Health
{
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly DapperContext _context;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _context.CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();
                return HealthCheckResult.Healthy();
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy(null, exception);
            }
        }
    }
}
