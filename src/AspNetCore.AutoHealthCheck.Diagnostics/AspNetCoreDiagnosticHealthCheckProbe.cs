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
using System.Threading.Tasks;

namespace AspNetCore.AutoHealthCheck.Diagnostics
{
    /// <summary>
    ///     AspNet Core diagnostic health check probe to call internal endpoint.
    /// </summary>
    internal class AspNetCoreDiagnosticHealthCheckProbe : IProbe
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public AspNetCoreDiagnosticHealthCheckProbe(
            IHttpClientFactory clientFactory,
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor)
        {
            _clientFactory = clientFactory;
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
        }

        /// <summary>
        ///     Path where diagnostic health check endpoint endpoint should be called.
        /// </summary>
        internal static string Path { get; set; }

        /// <inheritdoc />
        public string Name => "Microsoft.AspNetCore.Diagnostics.HealthChecks.Probe";

        public async Task<ProbeResult> Check()
        {
            var context = _autoHealthCheckContextAccessor.Context;
            var baseUtl = context.Configurations.BaseUrl;
            var endpointUrl = $"{baseUtl}{Path}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            switch (body.ToUpper())
            {
                case "HEALTHY":
                    return ProbeResult.Ok();

                default:
                    return ProbeResult.Error($"{Name} failed.");
            }
        }
    }
}