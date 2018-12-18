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

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace AspNetCore.AutoHealthCheck
{
    internal class RoutingScraper
    {
        public static IEnumerable<string> GetRouteConstraints(IRouteInformation info)
        {
            if (info.RouteTemplate == null)
                return new List<string>();

            // if the route template does not contains { or } then the route does not have any route param
            if (!info.RouteTemplate.Contains("{") || !info.RouteTemplate.Contains("}"))
                return new List<string>();

            return info.RouteTemplate.Split('{', '}')
                // remove those all with route symbol / and those all empty
                .Where(c => !string.IsNullOrWhiteSpace(c) && !c.Contains("/"));
        }

        public static void CoumpleteRoutingInformation(
            IRouteInformation info,
            ControllerActionDescriptor actionDescriptor)
        {
            if (info.RouteTemplate == null || actionDescriptor == null)
                return;

            var routeConstraints = GetRouteConstraints(info).ToList();
            if (!routeConstraints.Any())
                return;

            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();

            foreach (var routeParam in routeConstraints)
            {
                var param = methodParams.FirstOrDefault(p => p.Name == routeParam);
                if (param == null)
                    continue;

                // check if param has FromQuery or FromBody Attribute to avoid them
                if (param.ContainsAttribute<FromQueryAttribute>() || param.ContainsAttribute<FromBodyAttribute>())
                    continue;

                // add to the route information
                info.RouteParams[routeParam] = param.ParameterType;
            }
        }
    }
}