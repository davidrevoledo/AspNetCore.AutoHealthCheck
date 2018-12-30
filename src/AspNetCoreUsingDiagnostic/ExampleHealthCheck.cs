using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCoreUsingDiagnostic
{
    public class ExampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Execute health check logic here. This example sets a dummy
            // variable to true.
            var healthCheckResultHealthy = false;

            if (healthCheckResultHealthy)
                return Task.FromResult(
                    HealthCheckResult.Healthy("The check indicates a healthy result."));

            return Task.FromResult(
                HealthCheckResult.Degraded("The check indicates an unhealthy result."));
        }
    }
}