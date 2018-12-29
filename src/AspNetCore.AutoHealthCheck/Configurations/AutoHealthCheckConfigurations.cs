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
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Auto health check configurations
    /// </summary>
    public class AutoHealthCheckConfigurations : IAutoHealthCheckConfigurations
    {
        internal AutoHealthCheckConfigurations()
        {
            PassCheckRule = s => !Enumerable.Range(500, 599).Contains((int)s.StatusCode);
            DefaultUnHealthyResponseCode = HttpStatusCode.InternalServerError;
            DefaultHealthyResponseCode = HttpStatusCode.OK;
        }

        /// <inheritdoc />
        public List<Regex> ExcludeRouteRegexs { get; set; } = new List<Regex>();

        /// <inheritdoc />
        public Func<HttpResponseMessage, bool> PassCheckRule { get; set; }

        /// <inheritdoc />
        public HttpStatusCode DefaultUnHealthyResponseCode { get; set; }

        /// <inheritdoc />
        public HttpStatusCode DefaultHealthyResponseCode { get; set; }

        /// <inheritdoc />
        public AutomaticRunConfigurations AutomaticRunConfigurations { get; set; } = new AutomaticRunConfigurations();

        /// <inheritdoc />
        public List<IHealtCheckResultPlugin> ResultPlugins { get; set; } = new List<IHealtCheckResultPlugin>();

        /// <inheritdoc />
        public List<IHttpEndpointPlugin> HttpEndpointPlugins { get; set; } = new List<IHttpEndpointPlugin>();
    }
}