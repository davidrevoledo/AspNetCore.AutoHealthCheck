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
using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck.Extensibility;

namespace AspNetCore.AutoHealthCheck
{
    internal class InternalRouteInformationEvaluator : IInternalRouteInformationEvaluator
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IRouteEvaluator _routeEvaluator;

        public InternalRouteInformationEvaluator(
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor,
            IRouteEvaluator routeEvaluator)
        {
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
            _routeEvaluator = routeEvaluator;
        }

        /// <summary>
        ///     Internally evaluate a route to determine if needs to be included
        ///     to check
        /// </summary>
        /// <param name="routeInformation">route information</param>
        /// <returns>if the route candidate needs to be included</returns>
        public Task<bool> Evaluate(IRouteInformation routeInformation)
        {
            var context = _autoHealthCheckContextAccessor.Context;

            // check if the route template is included in one of the regex to exclude routes
            // if so them ingore it
            if (context.Configurations.ExcludeRouteRegexs.Any(e => e.IsMatch(routeInformation.RouteTemplate)))
                return Task.FromResult(false);

            return _routeEvaluator.Evaluate(routeInformation);
        }
    }
}