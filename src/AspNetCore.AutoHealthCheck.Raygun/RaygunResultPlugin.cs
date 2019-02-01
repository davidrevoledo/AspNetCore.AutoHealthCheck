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
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck.Raygun.Configurations;
using Mindscape.Raygun4Net;
using Newtonsoft.Json;

namespace AspNetCore.AutoHealthCheck.Raygun
{
    /// <summary>
    ///     Azure Storage Result Plugin to send failed checks to raygun
    /// </summary>
    public class RaygunResultPlugin : IHealthCheckResultPlugin
    {
        private readonly Lazy<RaygunClient> _raygunClient;

        public RaygunResultPlugin()
        {
            _raygunClient = new Lazy<RaygunClient>(CreateRaygunClient);
        }

        public string Name => "RaygunResultPlugin";

        /// <inheritdoc />
        public Task ActionAfterResult(HealthyResponse result)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ActionAfterSuccess(HealthyResponse result)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task ActionAfterFail(HealthyResponse result)
        {
            var configurations = HCRaygunConfigurations.Instance;
#if DEBUG
            if (configurations.AvoidSendInDebug) return;
#endif
            // build information
            var tags = configurations.Tags.ToList();
            tags.Add("sender : AspNetCore.AutoHealthCheck");

            var information = new Dictionary<string, string>
            {
                ["Body"] = JsonConvert.SerializeObject(result),
                ["Sender"] = "AspNetCore.AutoHealthCheck"
            };

            await _raygunClient.Value.SendInBackground(
                    new AspNetCoreAutoHealthCheckFailException(),
                    tags,
                    information)
                .ConfigureAwait(false);
        }

        private static RaygunClient CreateRaygunClient()
        {
            var configurations = HCRaygunConfigurations.Instance;

            if (string.IsNullOrWhiteSpace(configurations.ApiKey))
                throw new InvalidOperationException("Please Complete Raygun ApiKey");

            var raygunClient = new RaygunClient(configurations.ApiKey);

            return raygunClient;
        }
    }
}