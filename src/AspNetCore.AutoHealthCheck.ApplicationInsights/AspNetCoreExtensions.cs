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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.AutoHealthCheck.ApplicationInsights
{
    public static class AspNetCoreExtensions
    {
        /// <summary>
        ///     Add Application insights integration for results.
        /// </summary>
        /// <param name="healthChecksBuilder">aspnet auto health check builder.</param>
        /// <returns></returns>
        public static IAutoHealthCheckBuilder AddAIResultPlugin(
            this IAutoHealthCheckBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddSingleton<ApplicationInsightsResultPlugin>();
            healthChecksBuilder.AddSingleton<ITelemetryServices, TelemetryServices>();
            return healthChecksBuilder;
        }

        /// <summary>
        ///     Use Application insights integration for results.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="setupAction">allow user to change configuration for the plugin.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAIResultPlugin(
            this IApplicationBuilder app,
            Action<ApplicationInsightsPluginConfigurations> setupAction = null)
        {
            // get plugin and context
            var plugin = (ApplicationInsightsResultPlugin)app.ApplicationServices.GetService(
                typeof(ApplicationInsightsResultPlugin));

            var contextAccessor = (IAutoHealthCheckContextAccessor)app.ApplicationServices.GetService(
                typeof(IAutoHealthCheckContextAccessor));

            var context = contextAccessor.Context;
            context.Configurations.ResultPlugins.Add(plugin);

            // set configurations
            setupAction?.Invoke(ApplicationInsightsPluginConfigurations.Instance);

            return app;
        }
    }
}
