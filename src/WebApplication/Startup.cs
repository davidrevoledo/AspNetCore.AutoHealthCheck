using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication.Plugins;
using WebApplication.Probes;

namespace WebApplication
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
                    c.AutomaticRunConfigurations.BaseUrl = new Uri("http://localhost:50387");
                    c.AutomaticRunConfigurations.SecondsInterval = 1;
                    c.ResultPlugins.Add(new ResultPlugin());
                })
                .AddCustomProbe<CustomProbe>();
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
        }
    }
}