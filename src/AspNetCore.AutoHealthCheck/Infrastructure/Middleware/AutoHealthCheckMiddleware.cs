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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AspNetCore.AutoHealthCheck
{
    internal class AutoHealthCheckMiddleware
    {
        private readonly AutoHealthCheckMiddlewareOptions _appOptions;
        private readonly IHealthChecker _healthChecker;
        private readonly RequestDelegate _next;

        public AutoHealthCheckMiddleware(
            RequestDelegate next,
            IHealthChecker healthChecker,
            AutoHealthCheckMiddlewareOptions appOptions)
        {
            _next = next;
            _healthChecker = healthChecker;
            _appOptions = appOptions;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var path = httpContext.Request.Path.Value;

            // check request security
            if (_appOptions.SecurityHandler != null)
            {
                var validRequest = _appOptions.SecurityHandler.Invoke(httpContext.Request);
                if (!validRequest)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }

            if (httpMethod == "GET" && Regex.IsMatch(path, $"^/{_appOptions.RoutePrefix}/?$"))
            {
                var healthCheckResult = await _healthChecker.Check().ConfigureAwait(false);

                if (healthCheckResult == null)
                    throw new ArgumentNullException(nameof(healthCheckResult));

                httpContext.Response.StatusCode = (int)healthCheckResult.HttpStatus;
                httpContext.Response.ContentType = "application/json";
                var jsonString = JsonConvert.SerializeObject(healthCheckResult);
                await httpContext.Response.WriteAsync(jsonString, Encoding.UTF8).ConfigureAwait(false);

                return;
            }

            await _next(httpContext);
        }
    }
}