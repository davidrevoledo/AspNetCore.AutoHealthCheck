
AspNetCore.AutoHealtCheck
====================
Check automatically your asp.net core applications with a lot of extensibility !

[![CodeFactor](https://www.codefactor.io/repository/github/davidrevoledo/aspnetcore.autohealthcheck/badge)](https://www.codefactor.io/repository/github/davidrevoledo/aspnetcore.autohealthcheck)
[![Build status](https://ci.appveyor.com/api/projects/status/3s1txd8lrbvn82v2/branch/master?svg=true)](https://ci.appveyor.com/project/davidrevoledo/aspnetcore-autohealthcheck/branch/master)
![NuGet](https://img.shields.io/nuget/dt/AspNetCore.AutoHealthCheck.svg)
![NuGet](https://img.shields.io/nuget/v/AspNetCore.AutoHealthCheck.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# Contents
1. [Features](#features)
2. [Installation](#installation)
4. [Usage](#usage)
5. [Customising](#customising)
6. [License](#license)

## <a name="features"> Features </a>

-  Auto Call all the endpoints that an asp.net core applications expose.
-  Allow configure rules to determine when an endpoint is unhealthy.
-  Plugins to extend the behavior of the http calls. (ie. Headers, QueryParams, and so on).
-  Allow to call custom actions (ie weebhooks) for health resutls.
-  Full Async support.

## <a name="installation"> Installation </a>
Grab the latest Inyector NuGet package and install in your solution. https://www.nuget.org/packages/AspNetCore.AutoHealthCheck/
```sh
PM > Install-Package AspNetCore.AutoHealthCheck 
NET CLI - dotnet add package AspNetCore.AutoHealthCheck 
paket add AspNetCore.AutoHealthCheck 

```

## <a name="usage"> Usage </a>

In your asp.net core application Startup you just need to Add HealthCheck Service. That's it ! 

``` c#

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

            services.AddAutoHealthCheck();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }

```

You can check the result calling your host route + `/api/autoHealthCheck`
ie:  http://localhost:50387/api/autoHealthCheck






