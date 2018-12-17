using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace AspNetCore.AutoHealthCheck
{
    internal class Endpoint
    {
        private readonly string _host;
        private readonly IRouteInformation _routeInformation;

        public Endpoint(IRouteInformation routeInformation, string host)
        {
            _host = host;
            _routeInformation = routeInformation;
        }

        public HttpRequestMessage GetRequestCall()
        {
            var httpMethod = _routeInformation.GetHttpMethod();
            var endpointUrl = GetEndpointUrl();

            var request = new HttpRequestMessage(httpMethod, endpointUrl);

            request.Headers.Add("Accept", "application/json"); // todo : use accept
            request.Headers.Add("User-Agent", "AspNetCore.AutoHealthCheck");


            CompleteWithBodyRequest(request);

            return request;
        }

        private void CompleteWithBodyRequest(HttpRequestMessage request)
        {

            if (!_routeInformation.BodyParams.Any())
                return;

            var bodyParam = _routeInformation.BodyParams.First();

            string json;

            switch (bodyParam.Value)
            {
                case Type type when type == typeof(string):
                    json = "\"some string\"";
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

                case Type type when !type.IsAbstract && type.IsClass && (bodyParam.Value.GetConstructor(Type.EmptyTypes) != null):

                    var body = Activator.CreateInstance(bodyParam.Value);
                    json = JsonConvert.SerializeObject(body);
                    break;

                // not supported Nullable objects or others
                default:
                    return;
            }

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        public string GetEndpointUrl()
        {
            var routePath = $"{_host}{_routeInformation.Path}";

            routePath = TransformRouteParams(routePath);

            return routePath;
        }

        private string TransformRouteParams(string routePath)
        {
            // replace route objects
            foreach (var routeParam in _routeInformation.RouteParams)
            {
                var routeSection = $"{{{routeParam.Key}}}";

                // route params should be a primitive code
                switch (routeParam.Value)
                {
                    case Type type when type.IsNumericType():
                        // replace for a fake number
                        routePath = routePath.Replace(routeSection, 0.ToString());
                        break;

                    case Type type when type == typeof(string) || type == typeof(char):
                        // replace for a fake string
                        routePath = routePath.Replace(routeSection, "f");
                        break;

                    case Type type when type == typeof(bool):
                        routePath = routePath.Replace(routeSection, false.ToString());
                        break;

                    case Type type when type == typeof(DateTime):
                        var now = DateTime.UtcNow.ToString("o");
                        routePath = routePath.Replace(routeSection, WebUtility.UrlEncode(now));
                        break;

                        // for all Nullable types then null will be the default
                }
            }

            return routePath;
        }
    }
}