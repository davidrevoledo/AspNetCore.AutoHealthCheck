
AspNetCore.AutoHealtCheck
====================
Check automatically your asp.net core applications with a lot of extensibility !

[![CodeFactor](https://www.codefactor.io/repository/github/davidrevoledo/aspnetcore.autohealthcheck/badge)](https://www.codefactor.io/repository/github/davidrevoledo/aspnetcore.autohealthcheck)
![NuGet](https://img.shields.io/nuget/dt/AspNetCore.AutoHealthCheck.svg)
![NuGet](https://img.shields.io/nuget/v/AspNetCore.AutoHealthCheck.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

|Build/Package|Status|
|------|-------------|
|master|[![Build status](https://ci.appveyor.com/api/projects/status/3s1txd8lrbvn82v2/branch/master?svg=true)](https://ci.appveyor.com/project/davidrevoledo/aspnetcore-autohealthcheck/branch/master)
|dev| 

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
-  Automatic run on background using HostedServices
-  Full Async support.

## <a name="installation"> Installation </a>

Grab the latest AspNetCore.AutoHealthCheck NuGet package and install in your solution. https://www.nuget.org/packages/AspNetCore.AutoHealthCheck/
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

You will get a json with the health check response
with the following information:

- if health check was successfully.
- elapsed test time.
- endpoints who have failed.

``` JSON
{  
   "success":false,
   "elapsedSecondsTest":3,
   "unhealthyEndpoints":[  
      {  
         "route":"http://localhost:50387/api/Values/array",
         "httpStatusCode":500,
         "httpVerb":"POST"
      }
   ]
}
```

To activate background monitor just configure the options in startup.

``` C#
    services.AddAutoHealthCheck(c =>
    {
        c.AutomaticRunConfigurations.AutomaticRunEnabled = true;
        c.AutomaticRunConfigurations.BaseUrl = new Uri("http://localhost:50387");
        c.AutomaticRunConfigurations.SecondsInterval = 1;
    });
```
The url is required as asp.net core doesn't know exactly the URI if it is running behing a proxy reverse server like IIS in the moment the asp.net core application starts.

## <a name="customising"> Customising </a>

In order to customise the Check you can do the following:

``` c#
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // add the rest of your configurations.
            services.AddAutoHealthCheck(c =>
            {
                c.DefaultUnHealthyResponseCode = HttpStatusCode.Accepted;
                c.PassCheckRule = response => response.Content == null;
            });
        }
    }
```
There are the folowing configurations.
- `DefaultUnHealthyResponseCode` : Allow you to define the http code to return when a health check test has failed, defualt 500.

- `DefaultHealthyResponseCode` : Allow you to define the http code to return when the check was successfully.

- `PassCheckRule` : Allow you to define how to check if an endpoint is failing (for now is general) with this rule will check all the endpoints

default : Will fail if the endpoint return an status code between 500 - 599.
ie : ```c# c.PassCheckRule = response => !response.Headers.Contains("x-header"); ```

- `ExcludeRouteRegexs` : Allow you to define a collection of refex to exclude endpoints to be called for the check.

- `ResultPlugins` : Allow you to define plugins to do something custom with a health check results.  This is a really great feature as could open the door to plugins like a plugin that call a webhook each time the health check fail to do something like reset the service or send an email.

In order to implement those plugins you have to implement `IHealtCheckResultPlugin` interface and resolve one of or all the methods.

`ActionAfterResult` Do something after a any result.
`ActionAfterSuccess` Do something after a successfuly result.
`ActionAfterFail` Do something after a fail result.

Then just add the plugin to the configurations in any time of the application lifetime.

ie:  
``` C#
    public class ResultPlugin : IHealtCheckResultPlugin
    {
        private readonly ILogger _logger;

        public ResultPlugin(ILogger logger)
        {
            _logger = logger;
        }

        public string Name => "name";

        public Task ActionAfterResult(HealthyResponse result)
        {
            _logger.Log(LogLevel.Information, result.Success.ToString());
            return Task.CompletedTask;
        }

        public Task ActionAfterSuccess(HealthyResponse result)
        {
            return Task.CompletedTask;
        }

        public Task ActionAfterFail(HealthyResponse result)
        {
            return Task.CompletedTask;
        }
    }
```

- `HttpEndpointPlugins` : Allow you to change the request content that are sent to check the endpoints of your asp.net application, here you can add custom headers, or query strings, things that are neccesary to hit your endpoints.

To implement them you just have to implement this interface `IHttpEndpointPlugin` and call:
  BeforeSend
  AfterReceive
  
  with the full request /response.
 
 ``` C#
    public class ResultPlugin : IHttpEndpointPlugin
    {
        public string Name => throw new NotImplementedException();

        public Task<HttpResponseMessage> AfterReceive(HttpResponseMessage response)
        {
            return Task.FromResult(response);
        }

        public Task<HttpRequestMessage> BeforeSend(HttpRequestMessage request)
        {
            request.Headers.Add("custom-header", "value");

            return Task.FromResult(request);
        }
    }
```

If you want to avoid a controller / method to be called just need a filter `AvoidAutoHealtCheckAttribute`

 ``` C#
    [AvoidAutoHealtCheck]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [AvoidAutoHealtCheck]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
        
     }
```

  
## <a name="license"> License </a>

MIT License
Copyright (c) 2018 David Revoledo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Made with ❤ in [DGENIX](https://www.dgenix.com/)

