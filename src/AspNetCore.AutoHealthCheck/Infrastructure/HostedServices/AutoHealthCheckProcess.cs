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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Hosted service to auto call internally auto health check endpoint.
    /// </summary>
    internal class AutoHealthCheckProcess : IHostedService, IDisposable
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public AutoHealthCheckProcess(
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor,
            IHttpClientFactory clientFactory)
        {
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
            _clientFactory = clientFactory;
        }

        public void Dispose()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var context = _autoHealthCheckContextAccessor.Context;
            while (true)
            {
                var endpointUrl = $"{context.Configurations.BaseUrl}/{context.AppBuilderOptions.RoutePrefix}";
                var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);

                var client = _clientFactory.CreateClient();
                await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await Task.Delay(context.Configurations.AutomaticRunConfigurations.SecondsInterval * 1000,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}