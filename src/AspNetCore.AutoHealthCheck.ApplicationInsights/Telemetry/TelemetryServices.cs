//MIT License
//Copyright(c) 2017 David Revoledo

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
// Project Lead - David Revoledo davidrevoledo@d-genix.com

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace AspNetCore.AutoHealthCheck.ApplicationInsights
{
    // todo : add integration tests to it.
    /// <inheritdoc />
    internal class TelemetryServices : ITelemetryServices
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;

        public TelemetryServices(IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor)
        {
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
        }

        /// <inheritdoc />
        public Task TrackAvailability(HealthyResponse result)
        {
            var configurations = ApplicationInsightsPluginConfigurations.Instance;
            var context = _autoHealthCheckContextAccessor.Context;

            var telemetryClient = GetClient();
            telemetryClient.TrackAvailability(
                configurations.AvailabilityName,
                DateTimeOffset.UtcNow,
                TimeSpan.FromSeconds(result.ElapsedSecondsTest),
                context.Configurations.BaseUrl.ToString(),
                result.Success);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task TrackEvent(HealthyResponse result)
        {
            var configurations = ApplicationInsightsPluginConfigurations.Instance;
            var telemetryClient = GetClient();

            telemetryClient.TrackEvent(
                configurations.EventName,
                GetProperties(result));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task TrackException(HealthyResponse result)
        {
            var telemetryClient = GetClient();
            telemetryClient.TrackException(
                new AspNetCoreAutoHealthCheckFailException(),
                GetProperties(result));

            return Task.CompletedTask;
        }

        private TelemetryClient GetClient()
        {
            var configurations = ApplicationInsightsPluginConfigurations.Instance;

            var configuration = string.IsNullOrWhiteSpace(configurations.InstrumentationKey)
                ? TelemetryConfiguration.Active
                : new TelemetryConfiguration(configurations.InstrumentationKey);

            return new TelemetryClient(configuration);
        }

        private static IDictionary<string, string> GetProperties(HealthyResponse result)
        {
            return new Dictionary<string, string>
            {
                ["IsHealthy"] = result.Success.ToString(),
                ["Status"] = result.HttpStatus.ToString(),
                ["ElapsedTime"] = result.ElapsedSecondsTest.ToString(),
                ["UnhealthyProbes"] = result.UnhealthyProbes.Count.ToString(),
                ["UnhealthyEndpoints"] = result.UnhealthyEndpoints.Count.ToString(),
                ["MachineName"] = Environment.MachineName,
                ["Assembly"] = Assembly.GetEntryAssembly().GetName().Name
            };
        }
    }
}