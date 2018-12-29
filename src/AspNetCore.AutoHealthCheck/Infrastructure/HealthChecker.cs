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

namespace AspNetCore.AutoHealthCheck
{
    /// <inheritdoc cref="IHealthChecker" />
    /// <summary>
    ///     Default Health Checker engine to run over an asp.net core application
    /// </summary>
    internal sealed class HealthChecker : IDisposable, IHealthChecker
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IEndpointBuilder _endpointBuilder;
        private readonly IEndpointCaller _endpointCaller;
        private readonly IEndpointMessageTranslator _endpointMessageTranslator;
        private readonly AsyncLazy<IEnumerable<IRouteInformation>> _routesFactory;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly IServiceProvider _serviceProvider;

        private int _disposeSignaled;

        public HealthChecker(
            IRouteDiscover aspNetRouteDiscover,
            IEndpointBuilder endpointBuilder,
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor,
            IEndpointMessageTranslator endpointMessageTranslator,
            IEndpointCaller endpointCaller,
            IServiceProvider serviceProvider)
        {
            _endpointBuilder = endpointBuilder;
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
            _endpointMessageTranslator = endpointMessageTranslator;
            _endpointCaller = endpointCaller;
            _serviceProvider = serviceProvider;

            // route async
            _routesFactory = new AsyncLazy<IEnumerable<IRouteInformation>>(() => aspNetRouteDiscover.GetAllEndpoints());
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeSignaled, 1) != 0)
                return;

            _semaphore?.Release();
            Disposed = true;
        }

        /// <inheritdoc />
        public bool Disposed { get; set; }

        /// <inheritdoc />
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
                var routeInformation = routesDefinition as IRouteInformation[] ?? routesDefinition.ToArray();

                if (routeInformation.Any())
                    foreach (var route in routeInformation.AsParallel()
                        .WithMergeOptions(ParallelMergeOptions.FullyBuffered))
                    {
                        var endpoint = await _endpointBuilder.CreateFromRoute(route).ConfigureAwait(false);
                        var message = await _endpointMessageTranslator.Transform(endpoint).ConfigureAwait(false);

                        // call endpoint
                        var result = await _endpointCaller.Send(message).ConfigureAwait(false);
                        results.Add(result);
                    }


                // at this point task is finished
                var currentContext = _autoHealthCheckContextAccessor.Context;

                var probesResults = await ExecuteCustomProbes(currentContext).ConfigureAwait(false);

                // check test timing
                watcher.Stop();

                return await HealthCheckResultProcessor.ProcessResult(
                    currentContext,
                    watcher,
                    results.ToArray(),
                    probesResults.ToArray());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<IEnumerable<ProbeResult>> ExecuteCustomProbes(IAutoHealthCheckContext currentContext)
        {
            var executionResult = new List<ProbeResult>();

            if (!currentContext.Probes.Any())
                return executionResult;

            // if they should not run async or there is just 1 probe then iterate over them.
            if (!currentContext.Configurations.RunCustomProbesAsync || currentContext.Probes.Count == 1)
            {
                foreach (var probeType in currentContext.Probes)
                {
                    if (!(_serviceProvider.GetService(probeType) is IProbe probe))
                        continue;

                    var result = await probe.Check().ConfigureAwait(false);
                    executionResult.Add(result);
                }
            }
            else
            {
                // run them all at same time
                var tasks = currentContext.Probes.Select(p =>
                {
                    var probe = _serviceProvider.GetService(p) as IProbe;
                    return probe.Check();
                })
                .ToList();

                await Task.WhenAll(tasks).ConfigureAwait(false);

                var results = tasks.Select(t => t.Result);
                executionResult.AddRange(results);
            }

            return executionResult;
        }
    }
}