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
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Auto health check configurations
    /// </summary>
    public interface IAutoHealthCheckConfigurations
    {
        /// <summary>
        ///     Pass check rule to determine if a response is
        ///     Whit this method each endoint will be evaluated if the result was excpeted
        ///     Deafault is status code should be out from 500-599 range (Internal Server Errors)
        /// </summary>
        Func<HttpResponseMessage, bool> PassCheckRule { get; }

        /// <summary>
        ///     Regex to exclude routes
        ///     Each regex here will be evaluated foreach route to avoid them to be checked
        /// </summary>
        List<Regex> ExcludeRouteRegexs { get; }

        /// <summary>
        ///     Default http code to return when an endpoint fail default 500
        /// </summary>
        HttpStatusCode DefaultUnHealthyResponseCode { get; }

        /// <summary>
        ///     Default http code to return when all the endpoint are ok default 200
        /// </summary>
        HttpStatusCode DefaultHealthyResponseCode { get; }

        /// <summary>
        ///     Plugins to process results
        /// </summary>
        List<IHealtCheckResultPlugin> ResultPlugins { get; set; }

        /// <summary>
        ///     Http endpoints plugins to do some http transformation or completition
        ///     Like add custom headers
        /// </summary>
        List<IHttpEndpointPlugin> HttpEndpointPlugins { get; set; }

        /// <summary>
        ///     Automatic run configurations
        /// </summary>
        AutomaticRunConfigurations AutomaticRunConfigurations { get; }
    }
}