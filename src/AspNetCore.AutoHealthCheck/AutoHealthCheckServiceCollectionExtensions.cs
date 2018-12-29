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
using AspNetCore.AutoHealthCheck;
using AspNetCore.AutoHealthCheck.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoHealthCheckServiceCollectionExtensions
    {
        private static AutoHealthCheckContextAccessor _contextAccessor;

        /// <summary>
        ///     Add Auto health check to the asp.net core application without configurations
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">configurations</param>
        /// <returns></returns>
        public static IServiceCollection AddAutoHealthCheck(
            this IServiceCollection services,
            Action<AutoHealthCheckConfigurations> setupAction = null)
        {
            services.AddSingleton<IRouteDiscover, RouteDiscover>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IHealthChecker, HealthChecker>();
            services.AddSingleton<IEndpointBuilder, EndpointBuilder>();
            services.AddSingleton<IInternalRouteInformationEvaluator, InternalRouteInformationEvaluator>();
            services.AddSingleton<IRouteEvaluator, DefaultRouteEvaluator>();
            services.AddSingleton<IEndpointMessageTranslator, EndpointMessageTranslator>();
            services.AddSingleton<IEndpointCaller, EndpointCaller>();
            services.AddHttpClient();

            // resolve options
            var options = new AutoHealthCheckConfigurations();
            setupAction?.Invoke(options);
            _contextAccessor = new AutoHealthCheckContextAccessor();
            _contextAccessor.SetConfigurations(options);
            services.AddSingleton<IAutoHealthCheckContextAccessor>(_contextAccessor);

            // check if the service need to run automatically
            if (options.AutomaticRunConfigurations.AutomaticRunEnabled)
                services.AddSingleton<IHostedService, AutoHealthCheckProcess>();

            return services;
        }

        /// <summary>
        ///     Add custom probe work.
        /// </summary>
        /// <typeparam name="TProbe">Probe type to add to the work queue.</typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCustomProbe<TProbe>(this IServiceCollection services)
            where TProbe : class, IProbe
        {   
            if (_contextAccessor == null)
                throw new InvalidOperationException("Please first call AddAutoHealthCheck method.");

            // register probe in the IOC engine.
            services.AddTransient(typeof(TProbe));

            // add the probe to context
            var context = (AutoHealthCheckContext)_contextAccessor.Context;
            context.AddProbe<TProbe>();

            return services;
        }
    }
}