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

using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.AutoHealthCheck
{
    internal class AutoHealtCheckProcess : IHostedService, IDisposable
    {
        private readonly IAutoHealthCheckContextAccesor _autoHealthCheckContextAccesor;
        private readonly IHttpClientFactory _clientFactory;

        public AutoHealtCheckProcess(
            IAutoHealthCheckContextAccesor autoHealthCheckContextAccesor,
            IHttpClientFactory clientFactory)
        {
            _autoHealthCheckContextAccesor = autoHealthCheckContextAccesor;
            _clientFactory = clientFactory;
        }

        public void Dispose()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var context = _autoHealthCheckContextAccesor.Context;
            while (true)
            {
                var client = _clientFactory.CreateClient();

                var endpointUrl = $"{context.Configurations.AutomaticRunConfigurations.BaseUrl}/api/autoHealthCheck";
                var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
                await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                await Task.Delay(context.Configurations.AutomaticRunConfigurations.SecondsInterval * 1000, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
