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
using Microsoft.AspNetCore.Mvc.Controllers;

namespace AspNetCore.AutoHealthCheck
{
    internal class QueryStringScraper
    {
        public static IRouteInformation CompleteQueryStringRequiredParams(
            IRouteInformation info,
            ControllerActionDescriptor actionDescriptor)
        {
            // find all parameteres who don't belong to the route template and are not nullable
            var methodInfo = actionDescriptor.MethodInfo;
            var methodParams = methodInfo.GetParameters().ToList();
            var routeConstraints = RoutingScraper.GetRouteConstraints(info).ToList();

            bool IsAlreadyIncluded(MemberInfo requestedType)
            {
                // save them all who are not body params or constraints with or without FromQuery
                return routeConstraints.Contains(requestedType.Name) ||
                       info.BodyParams.Any(b => b.Key == requestedType.Name);
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
                    var getObjectProperties = param.ParameterType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
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

            return info;
        }
    }
}