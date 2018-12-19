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

using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck.Infraestructure.Plugins;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Endpoint plugins to allow extend how messages are sent or received
    ///     ie : add custom headers etc
    /// </summary>
    public interface IHttpEndpointPlugin : IPlugin
    {
        /// <summary>
        ///     Do something before send the http message
        /// </summary>
        /// <param name="request">http message</param>
        /// <returns></returns>
        Task<HttpRequestMessage> BeforeSend(HttpRequestMessage request);

        /// <summary>
        ///     Do something after receive the http response from endpoint
        /// </summary>
        /// <param name="response">response for endpoint</param>
        /// <returns></returns>
        Task<HttpResponseMessage> AfterReceive(HttpResponseMessage response);
    }
}
