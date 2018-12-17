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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Discover all the endpoints that an asp.net core application expose
    /// </summary>
    public class AspNetRouteDiscover : IAspNetRouteDiscover
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public AspNetRouteDiscover(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        /// <summary>
        ///     Get routes information an asp.net core application expose
        /// </summary>
        /// <returns>collection of route information</returns>
        public IEnumerable<IRouteInformation> GetAllEndpoints()
        {
            var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items;
            foreach (var action in routes)
            {
                //  Check if the action needs to be ignored
                if (CheckIfRouteShouldBeIgnored(action)) continue;

                var info = new RouteInformation
                {
                    RouteTemplate = action.AttributeRouteInfo?.Template
                };

                // Path and Invocation of Razor Pages
                if (action is PageActionDescriptor pageAction)
                {
                    info.Path = pageAction.ViewEnginePath;
                }

                // Path of Route Attribute
                if (action.AttributeRouteInfo != null)
                {
                    info.Path = $"/{action.AttributeRouteInfo.Template}";
                }

                // Path and Invocation of Controller/Action
                if (action is ControllerActionDescriptor actionDescriptor)
                {
                    if (string.IsNullOrWhiteSpace(info.Path))
                    {
                        info.Path = $"/{actionDescriptor.ControllerName}/{actionDescriptor.ActionName}";
                    }

                    // complete route params for transformation
                    CompleteRouteParams(info, actionDescriptor);

                    // complete query strings params
                    CompleteQueryStringParams(info, actionDescriptor);

                    // complete fromBody params
                    CompleteFromBodyParams(info, actionDescriptor);
                }

                // Extract HTTP Verb
                if (action.ActionConstraints != null)
                {
                    var constraintTypes = action.ActionConstraints.Select(t => t.GetType()).ToList();

                    // Check Http method constraint
                    if (constraintTypes.Contains(typeof(HttpMethodActionConstraint)))
                    {
                        var httpVerbConstrain =
                            action.ActionConstraints.FirstOrDefault(a =>
                                a.GetType() == typeof(HttpMethodActionConstraint));

                        if (httpVerbConstrain is HttpMethodActionConstraint httpMethodAction)
                            info.HttpMethod = string.Join(",", httpMethodAction.HttpMethods);
                    }
                }

                yield return info;
            }
        }

        private void CompleteFromBodyParams(RouteInformation info, ControllerActionDescriptor actionDescriptor)
        {
            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();

            foreach (var param in methodParams)
            {
                // check if param has FromQuery or FromBody Attribute to avoid them
                if (param.GetCustomAttributes(typeof(FromBodyAttribute), false).Any())
                {
                    // add to the body information and break as 1 only will be supported
                    info.BodyParams[param.Name] = param.ParameterType;
                    break;
                }
            }
        }

        private void CompleteQueryStringParams(RouteInformation info, ControllerActionDescriptor actionDescriptor)
        {
            // find all parameteres who don't belong to the route template and are not nullable
        }

        private static void CompleteRouteParams(IRouteInformation info, ControllerActionDescriptor actionDescriptor)
        {
            if (info.RouteTemplate == null)
                return;

            // if the route template does not contains { or } then the route does not have any route param
            if (!info.RouteTemplate.Contains("{") || !info.RouteTemplate.Contains("}"))
                return;

            // get-activities{id}/done/{filter}
            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();

            var splitedRoute = info.RouteTemplate.Split('{', '}');
            foreach (var routeParam in splitedRoute)
            {
                if (string.IsNullOrWhiteSpace(routeParam))
                    continue;

                var param = methodParams.FirstOrDefault(p => p.Name == routeParam);

                if (param == null)
                    continue;

                // check if param has FromQuery or FromBody Attribute to avoid them
                if (param.GetCustomAttributes(typeof(FromQueryAttribute), false).Any() ||
                    param.GetCustomAttributes(typeof(FromBodyAttribute), false).Any())
                    continue;

                // add to the route information
                info.RouteParams[routeParam] = param.ParameterType;
            }
        }

        private static bool CheckIfRouteShouldBeIgnored(ActionDescriptor action)
        {
            foreach (var filter in action.FilterDescriptors)
            {
                var ignoredType = typeof(AvoidAutoHealtCheckAttribute);

                if (filter.Filter is Attribute attribute && attribute.GetType() == ignoredType)
                    return true;
            }

            return false;
        }
    }
}