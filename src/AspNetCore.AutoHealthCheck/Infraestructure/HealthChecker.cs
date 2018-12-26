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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck.Infraestructure;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Deafult Healt Checker engine to run over an asp.net core application
    /// </summary>
    internal sealed class HealthChecker : IDisposable, IHealthChecker
    {
        private readonly IAutoHealthCheckContextAccesor _autoHealthCheckContextAccesor;
        private readonly IEndpointBuilder _endpointBuilder;
        private readonly IEndpointMessageTranslator _endpointMessageTranslator;
        private readonly AsyncLazy<IEnumerable<IRouteInformation>> _routesFactory;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly IEndpointCaller _endpointCaller;

        private int _disposeSignaled;

        public HealthChecker(
            IRouteDiscover aspNetRouteDiscover,
            IEndpointBuilder endpointBuilder,
            IAutoHealthCheckContextAccesor autoHealthCheckContextAccesor,
            IEndpointMessageTranslator endpointMessageTranslator, 
            IEndpointCaller endpointCaller)
        {
            _endpointBuilder = endpointBuilder;
            _autoHealthCheckContextAccesor = autoHealthCheckContextAccesor;
            _endpointMessageTranslator = endpointMessageTranslator;
            _endpointCaller = endpointCaller;

            // route async
            _routesFactory = new AsyncLazy<IEnumerable<IRouteInformation>>(() => aspNetRouteDiscover.GetAllEndpoints());
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
        public async Task<HealthyResponse> Check()
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var watcher = Stopwatch.StartNew();
                var results = new List<HttpResponseMessage>();

                var routesDefinition = await _routesFactory;
                var routeInformations = routesDefinition as IRouteInformation[] ?? routesDefinition.ToArray();

                if (routeInformations.Any())
                {
                    // procecess in paraller
                    foreach (var route in routeInformations.AsParallel()
                        .WithMergeOptions(ParallelMergeOptions.FullyBuffered))
                    {
                        var endpoint = await _endpointBuilder.CreateFromRoute(route).ConfigureAwait(false);
                        var message = await _endpointMessageTranslator.Transform(endpoint).ConfigureAwait(false);

                        // call endpoint
                        var result = await _endpointCaller.Send(message).ConfigureAwait(false);
                        results.Add(result);
                    }
                }

                // check test timing
                watcher.Stop();

                // at this point task is finished
                var currentContext = _autoHealthCheckContextAccesor.Context;
                return await HealtCheckResultProcessor.ProcessResult(currentContext, watcher, results.ToArray());
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}