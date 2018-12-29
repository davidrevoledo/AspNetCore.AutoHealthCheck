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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Endpoint builder
    /// </summary>
    internal class EndpointBuilder : IEndpointBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EndpointBuilder(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        ///     Create endpoint definition from route
        /// </summary>
        /// <param name="routeInformation">route information</param>
        /// <returns>endpoint definition</returns>
        public Task<IEndpoint> CreateFromRoute(IRouteInformation routeInformation)
        {
            if (routeInformation == null) throw new ArgumentNullException(nameof(routeInformation));

            var httpContext = _httpContextAccessor.HttpContext;
            var host = $@"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            IEndpoint endpoint = new Endpoint(routeInformation, host);
            return Task.FromResult(endpoint);
        }
    }
}