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
using AspNetCore.AutoHealthCheck.ApplicationInsights;
using AspNetCore.AutoHealthCheck.AzureStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleWebApp.Plugins;
using SimpleWebApp.Probes;

namespace SimpleWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAutoHealthCheck(c =>
                {
                    c.AutomaticRunConfigurations.AutomaticRunEnabled = false;
                    c.BaseUrl = new Uri("http://localhost:51580");
                    c.AutomaticRunConfigurations.SecondsInterval = 1;
                    c.ResultPlugins.Add(new ResultPlugin());
                })
                .AddCustomProbe<CustomProbe>()
                .AddAIResultPlugin()
                .AddAzureStorageIntegration();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvc();
            app.UseAutoHealthCheck(c =>
            {
                c.RoutePrefix = "insights/healthcheck";
                c.SecurityHandler = request => request.Query.ContainsKey("key") && request.Query["key"] == "1234";
            });
            app.UseAIResultPlugin(s =>
            {
                s.InstrumentationKey = "Your Key Here";
                s.Mode = TrackMode.Event;
            });

            var storageCs =
                @"DefaultEndpointsProtocol=https;AccountName=testaspnetcorehealt;AccountKey=hHscD888d19ETGKjBXYTgEW3t1JYTb7FlP2q9OuHrpYPBgqYhryemDioRvWsUbzQCRPmHFwit0fOVcotVV1XyQ==;EndpointSuffix=core.windows.net";
            app.UseStorageHealthCheckIntegration(storageCs, c =>
            {
                c.OnlyTrackFailedResults = false;
            });
        }
    }
}