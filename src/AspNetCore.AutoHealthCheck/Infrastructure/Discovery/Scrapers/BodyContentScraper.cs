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

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace AspNetCore.AutoHealthCheck
{
    internal class BodyContentScraper
    {
        public static IRouteInformation CompleteBodyRequiredContent(
            IRouteInformation info,
            ControllerActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
                return info;

            // avoid get and delete methods
            if (info.HttpMethod == "GET" || info.HttpMethod == "DELETE")
                return info;

            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();
            var routeConstraints = RoutingScraper.GetRouteConstraints(info).ToList();

            // first check if there is any param marked as body to give it more priority even if they are not the first param
            foreach (var param in methodParams)
                // check if param has FromQuery or FromBody Attribute to avoid them
                if (param.ContainsAttribute<FromBodyAttribute>() && !routeConstraints.Contains(param.Name))
                {
                    info.BodyParams[param.Name] = param.ParameterType;
                    break;
                }

            // only 1 will be supported for now
            if (info.BodyParams.Any())
                return info;

            // now check if there is params not marked as body who does not belong to the route constraint 
            // part of the route.
            // this is done because Asp.Net Core support getting object with HttPost without marking them with [FromBody]
            foreach (var param in methodParams)
                // it will take the first one always
                if (!routeConstraints.Contains(param.Name))
                {
                    info.BodyParams[param.Name] = param.ParameterType;
                    break;
                }

            return info;
        }
    }
}