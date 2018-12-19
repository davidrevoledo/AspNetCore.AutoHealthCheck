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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Deafult Healt Checker engine to run over an asp.net core application
    /// </summary>
    internal sealed class HealthChecker : IDisposable, IHealthChecker
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly Lazy<IEnumerable<IRouteInformation>> _routes;
        private readonly IEndpointBuilder _endpointBuilder;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly IAutoHealthCheckContextAccesor _autoHealthCheckContextAccesor;

        private int _disposeSignaled;

        public HealthChecker(
            IRouteDiscover aspNetRouteDiscover,
            IHttpClientFactory clientFactory,
            IEndpointBuilder endpointBuilder,
            IAutoHealthCheckContextAccesor autoHealthCheckContextAccesor)
        {
            _clientFactory = clientFactory;
            _endpointBuilder = endpointBuilder;
            _autoHealthCheckContextAccesor = autoHealthCheckContextAccesor;
            _routes = new Lazy<IEnumerable<IRouteInformation>>(aspNetRouteDiscover.GetAllEndpoints);
        }

        /// <summary>
        ///     Dispose the resource
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeSignaled, 1) != 0)
                return;

            _semaphore?.Release();
            Disposed = true;
        }

        /// <summary>
        ///     Indicate if the resource was disposed
        /// </summary>
        public bool Disposed { get; set; }

        /// <summary>
        ///     Perform the health check
        /// </summary>
        /// <returns>Response with the test result</returns>
        public async Task<IActionResult> Check()
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var watcher = Stopwatch.StartNew();
                var results = new List<Task<HttpResponseMessage>>();

                if (_routes.Value.Any())
                {
                    var client = _clientFactory.CreateClient();

                    results = _routes.Value.Select(route =>
                   {
                       var request = _endpointBuilder.CreateFromRoute(route)
                           .GetRequestCall();

                       return client.SendAsync(request);
                   }).ToList();

                    await Task.WhenAll(results).ConfigureAwait(false);
                }

                // check test timing
                watcher.Stop();

                // at this point task is finished
                var currentContext = _autoHealthCheckContextAccesor.Context;
                return HealtCheckResultProcessor.ProcessResult(currentContext, watcher, results.Select(r => r.Result).ToArray());
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}