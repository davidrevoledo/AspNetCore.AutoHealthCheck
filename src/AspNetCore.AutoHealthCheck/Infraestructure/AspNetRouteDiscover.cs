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
using System.Reflection;
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

                //  when need to update this after the http completition 
                if (action is ControllerActionDescriptor controllerAction)
                {
                    // complete route params for transformation
                    RoutingScraper.CoumpleteRoutingInformation(info, controllerAction);

                    // complete fromBody params
                    CompleteFromBodyParams(info, controllerAction);

                    // complete query strings params
                    CompleteQueryStringParams(info, controllerAction);
                }

                yield return info;
            }
        }

        private static void CompleteFromBodyParams(IRouteInformation info, ControllerActionDescriptor actionDescriptor)
        {
            // avoid get and delete methods
            if (info.HttpMethod == "GET" || info.HttpMethod == "DELETE")
                return;

            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();

            // first check if there is any param marked as body to give it more priority
            foreach (var param in methodParams)
            {
                // check if param has FromQuery or FromBody Attribute to avoid them
                if (param.ContainsAttribute<FromBodyAttribute>())
                {
                    info.BodyParams[param.Name] = param.ParameterType;
                    break;
                }
            }

            // only 1 will be supported for now
            if (info.BodyParams.Any())
                return;

            // now check if there is params not marked as body who does not belong to the route constraint 
            // part of the route.
            // this is done because Asp.Net Core support getting object with HttPost without marking them with [FromBody]
            var routeConstraints = RoutingScraper.GetRouteConstraints(info).ToList();

            foreach (var param in methodParams)
            {
                // it will take the first one always
                if (!routeConstraints.Contains(param.Name))
                {
                    info.BodyParams[param.Name] = param.ParameterType;
                    break;
                }
            }
        }

        private static void CompleteQueryStringParams(IRouteInformation info, ControllerActionDescriptor actionDescriptor)
        {
            // find all parameteres who don't belong to the route template and are not nullable
            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();
            var routeConstraints = RoutingScraper.GetRouteConstraints(info).ToList();

            bool IsAlreadyIncluded(MemberInfo requestedType)
            {
                // save them all who are not body params or constraints with or without FromQuery
                return routeConstraints.Contains(requestedType.Name) || info.BodyParams.Any(b => b.Key == requestedType.Name);
            }

            bool IsSupported(Type requestedType)
            {
                // check if the type is one of the supported for querystring
                var notNumericSupportedQueryStringTypes = new List<Type>
                {
                    typeof(string),
                    typeof(char),
                    typeof(DateTime),
                    typeof(bool)
                };

                return notNumericSupportedQueryStringTypes.Contains(requestedType) || requestedType.IsNumericType();
            }

            foreach (var param in methodParams)
            {
                // ignore existing ones
                if (IsAlreadyIncluded(param.ParameterType))
                    continue;

                var supportedType = IsSupported(param.ParameterType);
                if (supportedType)
                {
                    info.QueryParams[param.Name] = param.ParameterType;
                }
                else if (param.ParameterType.IsClass && !param.ParameterType.IsAbstract &&
                         param.ParameterType.SupportParameterLessConstructor() &&
                         param.ContainsAttribute<FromQueryAttribute>())
                {
                    // this is a complex object marked with [FromQuery]
                    // asp.net core let our get complex objects in get http fashion without specify one by one the params by using a complex class
                    // and mark it with [FromQuery]
                    var getObjectProperties = param.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(o => o.CanWrite && o.CanRead)
                        .ToList();

                    foreach (var property in getObjectProperties)
                    {
                        // ignore existing ones
                        if (IsAlreadyIncluded(property.PropertyType))
                            continue;

                        // we don't need recursivity for complex type as asp.net core does not support it
                        if (!IsSupported(property.PropertyType))
                            continue;

                        info.QueryParams[property.Name] = property.PropertyType;
                    }
                }
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