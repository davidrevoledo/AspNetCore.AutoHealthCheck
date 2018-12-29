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
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Endpoint translator to get a HttpRequest message to call the endpoint
    /// </summary>
    internal class EndpointMessageTranslator : IEndpointMessageTranslator
    {
        /// <summary>
        ///     Convert an endpoint to represent a HttpRequest message to call it
        /// </summary>
        /// <param name="endpoint">endpoint information</param>
        /// <returns>Http request to call the endpoint</returns>
        public Task<HttpRequestMessage> Transform(IEndpoint endpoint)
        {
            var httpMethod = endpoint.RouteInformation.GetHttpMethod();
            var endpointUrl = GetEndpointUrl(endpoint);

            var request = new HttpRequestMessage(httpMethod, endpointUrl);

            request.Headers.Add("User-Agent", "AspNetCore.AutoHealthCheck");

            // todo : configure headers
            request.Headers.Add("Accept", "application/json");

            CompleteWithBodyRequest(endpoint, request);
            return Task.FromResult(request);
        }

        private static void CompleteWithBodyRequest(IEndpoint endpoint, HttpRequestMessage request)
        {
            if (!endpoint.RouteInformation.BodyParams.Any())
                return;

            var bodyParam = endpoint.RouteInformation.BodyParams.First();

            string json;
            switch (bodyParam.Value)
            {
                case Type type when type == typeof(string) || type == typeof(char):
                    json = "\"f\"";
                    break;

                case Type type when type == typeof(bool):
                    json = false.ToString();
                    break;

                case Type type when type.IsNumericType():
                    json = 0.ToString();
                    break;

                case Type type when type == typeof(DateTime):
                    json = "\"" + DateTime.UtcNow.ToString("o") + "\"";
                    break;

                case Type type when !type.IsAbstract && type.IsClass &&
                                    bodyParam.Value.SupportParameterLessConstructor():
                    var body = Activator.CreateInstance(bodyParam.Value);
                    json = JsonConvert.SerializeObject(body);
                    break;

                // not supported Nullable objects or others
                default:
                    return;
            }

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static string GetEndpointUrl(IEndpoint endpoint)
        {
            // heal in case endpoint not start with /
            var endpointResourece = endpoint.RouteInformation.Path;
            if (!endpointResourece.StartsWith("/"))
                endpointResourece = $"/" + endpointResourece;

            var routePath = $"{endpoint.Host}{endpointResourece}";
            routePath = TransformRouteParams(endpoint, routePath);

            var routeBuilder = new StringBuilder(routePath);

            // complete query string
            var queryValues = new Dictionary<string, string>();
            foreach (var queryParam in endpoint.RouteInformation.QueryParams)
            {
                var queryValueToReplace = GetDefaultJsonValue(queryParam.Value);

                if (string.IsNullOrWhiteSpace(queryValueToReplace))
                    continue;

                queryValues.Add(queryParam.Key, WebUtility.UrlEncode(queryValueToReplace));
            }

            var first = true;
            foreach (var item in queryValues)
            {
                if (first)
                {
                    routeBuilder.Append('?');
                    first = false;
                }
                else
                {
                    routeBuilder.Append('&');
                }

                routeBuilder.Append(item.Key);
                routeBuilder.Append('=');
                routeBuilder.Append(item.Value);
            }

            return routeBuilder.ToString();
        }

        private static string TransformRouteParams(IEndpoint endpoint, string routePath)
        {
            if (!endpoint.RouteInformation.RouteParams.Any())
                return routePath;

            var routeResult = new StringBuilder(routePath);

            foreach (var routeParam in endpoint.RouteInformation.RouteParams)
            {
                var routeSection = $"{{{routeParam.Key}}}";
                var routeValue = GetDefaultJsonValue(routeParam.Value);

                if (string.IsNullOrWhiteSpace(routeValue))
                    continue;

                routeResult.Replace(routeSection, routeValue);
            }

            return routeResult.ToString();
        }

        private static string GetDefaultJsonValue(Type valueType)
        {
            switch (valueType)
            {
                case Type type when type.IsNumericType():
                    // replace for a fake number
                    return 0.ToString();

                case Type type when type == typeof(string) || type == typeof(char):
                    // replace for a fake string
                    return "f";

                case Type type when type == typeof(bool):
                    return false.ToString();

                case Type type when type == typeof(DateTime):
                    return DateTime.UtcNow.ToString("o");

                // for all Nullable types then null will be the default
                default:
                    return string.Empty;
            }
        }
    }
}