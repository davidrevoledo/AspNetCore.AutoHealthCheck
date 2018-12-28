﻿//MIT License
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

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Engine who call the endpoints via http
    /// </summary>
    internal class EndpointCaller : IEndpointCaller
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public EndpointCaller(
            IHttpClientFactory clientFactory,
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor)
        {
            _clientFactory = clientFactory;
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
        }

        /// <summary>
        ///     Call an http message from an endpoint
        /// </summary>
        /// <param name="message">message</param>
        /// <returns>response</returns>
        public async Task<HttpResponseMessage> Send(HttpRequestMessage message)
        {
            var context = _autoHealthCheckContextAccessor.Context;

            // process before send actions
            foreach (var httpPlugin in context.Configurations.HttpEndpointPlugins)
                message = await httpPlugin.BeforeSend(message).ConfigureAwait(false);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(message).ConfigureAwait(false);

            // process after receive actions
            foreach (var httpPlugin in context.Configurations.HttpEndpointPlugins)
                response = await httpPlugin.AfterReceive(response).ConfigureAwait(false);

            return response;
        }
    }
}