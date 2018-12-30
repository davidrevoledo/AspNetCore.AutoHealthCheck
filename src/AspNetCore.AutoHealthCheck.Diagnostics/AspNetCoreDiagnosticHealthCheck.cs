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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace AspNetCore.AutoHealthCheck.Diagnostics
{
    /// <summary>
    ///     This implementation allows call AutoHealthCheck endpoint with a Diagnostic health check probe.
    /// </summary>
    internal class AspNetCoreDiagnosticHealthCheck : IHealthCheck
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public AspNetCoreDiagnosticHealthCheck(
            IHttpClientFactory clientFactory,
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor)
        {
            _clientFactory = clientFactory;
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var autoHealthCheckContext = _autoHealthCheckContextAccessor.Context;

            // if auto health check is not enabled yet avoid failing
            if (autoHealthCheckContext == null)
                return HealthCheckResult.Healthy();

            var baseUtl = autoHealthCheckContext.Configurations.BaseUrl;
            var prefix = autoHealthCheckContext.AppBuilderOptions.RoutePrefix;
            var endpointUrl = $"{baseUtl}{prefix}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);

            var client = _clientFactory.CreateClient();
            var result = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var healthCheckResult = JsonConvert.DeserializeObject<HealthyResponse>(body);

            return healthCheckResult.Success
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"AspNetCore.AutoHealthCheck was not successfully. logs : {body}");
        }
    }
}