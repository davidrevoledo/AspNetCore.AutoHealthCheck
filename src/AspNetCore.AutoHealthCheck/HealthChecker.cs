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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Deafult Healt Checker engine to run over an asp.net core application
    /// </summary>
    internal sealed class HealthChecker : IDisposable, IHealthChecker
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<IEnumerable<IRouteInformation>> _routes;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private int _disposeSignaled;

        public HealthChecker(
            IAspNetRouteDiscover aspNetRouteDiscover,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientFactory = clientFactory;
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

                // get meta endpoints information
                var httpContext = _httpContextAccessor.HttpContext;
                var host = $@"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

                var client = _clientFactory.CreateClient();
                var watcher = Stopwatch.StartNew();

                var results = _routes.Value.Select(route =>
                {
                    var endpointUrl = $"{host}{route.Path}";
                    var httpMethod = route.GetHttpMethod();

                    var request = new HttpRequestMessage(httpMethod, endpointUrl);
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("User-Agent", "AspNetCore.AutoHealthCheck");

                    return client.SendAsync(request);
                }).ToList();

                await Task.WhenAll(results).ConfigureAwait(false);

                // check test timing
                watcher.Stop();

                // at this point task is finished
                return ReturnCheckResult(watcher, results.Select(r => r.Result).ToArray());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static IActionResult ReturnCheckResult(Stopwatch watcher, HttpResponseMessage[] results)
        {
            HttpStatusCode testStatusCodeResult;

            var healtyResponse = new HealthyResponse
            {
                ElapsedSecondsTest = watcher.ElapsedMilliseconds / 1000
            };

            // check if all responses are out of error server range
            if (results.All(r => !Enumerable.Range(500, 599).Contains((int)r.StatusCode)))
            {
                healtyResponse.Success = true;
                testStatusCodeResult = HttpStatusCode.OK;
            }
            else
            {
                testStatusCodeResult = HttpStatusCode.InternalServerError;

                // build the unhealthy response including all the failing endpoints
                foreach (var result in results)
                {
                    if (!Enumerable.Range(500, 599).Contains((int)result.StatusCode))
                        continue;

                    healtyResponse.UnhealthyEndpoints.Add(new UnhealthyEndpoint
                    {
                        HttpStatusCode = (int)result.StatusCode,
                        Route = result.RequestMessage.RequestUri.ToString()
                    });
                }
            }

            return new JsonResult(healtyResponse)
            {
                StatusCode = (int)testStatusCodeResult
            };
        }
    }
}