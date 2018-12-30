using System;
using AspNetCore.AutoHealthCheck.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreUsingDiagnostic
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
            services.AddHealthChecks()
                .AddCheck<ExampleHealthCheck>("example_health_check");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAutoHealthCheck(c =>
            {
                c.BaseUrl = new Uri("http://localhost:49987");
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHealthChecks("/health");
            app.UseMvc();
            app.UseAutoHealthCheck(c =>
            {
                c.RoutePrefix = "insights/health";
            });

            app.AddDiagnosticsHealthChecksIntegration();
        }
    }
}