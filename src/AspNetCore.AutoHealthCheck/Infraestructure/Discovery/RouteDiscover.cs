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
    internal class RouteDiscover : IRouteDiscover
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly IInternalRouteInformationEvaluator _internalRouteInformationEvaluator;

        public RouteDiscover(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IInternalRouteInformationEvaluator internalRouteInformationEvaluator)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _internalRouteInformationEvaluator = internalRouteInformationEvaluator;
        }

        /// <summary>
        ///     Get routes information an asp.net core application expose
        /// </summary>
        /// <returns>collection of route information</returns>
        public IEnumerable<IRouteInformation> GetAllEndpoints()
        {
            var candidates = GetEndpointsCandidates();

            // after get all route candidates then evaluate them all to return only those who pass 
            // the evaluation
            return candidates.Where(c => _internalRouteInformationEvaluator.Evaluate(c));
        }

        private IEnumerable<IRouteInformation> GetEndpointsCandidates()
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
                if (action is PageActionDescriptor pageAction) info.Path = pageAction.ViewEnginePath;

                // Path of Route Attribute
                if (action.AttributeRouteInfo != null) info.Path = $"/{action.AttributeRouteInfo.Template}";

                // Path and Invocation of Controller/Action
                if (action is ControllerActionDescriptor actionDescriptor)
                    if (string.IsNullOrWhiteSpace(info.Path))
                        info.Path = $"/{actionDescriptor.ControllerName}/{actionDescriptor.ActionName}";

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
                    RoutingScraper.CoumpleteRoutingInformation(info, controllerAction);
                    BodyContentScraper.CompleteBodyRequiredContent(info, controllerAction);
                    QueryStringScraper.CompleteQueryStringRequiredParams(info, controllerAction);
                }

                yield return info;
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