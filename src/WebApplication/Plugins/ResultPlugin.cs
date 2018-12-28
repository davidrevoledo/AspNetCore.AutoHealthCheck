using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck;
using Newtonsoft.Json;

namespace WebApplication.Plugins
{
    public class ResultPlugin : IHealtCheckResultPlugin
    {
        public string Name => "TestResultPlugin";

        public Task ActionAfterFail(HealthyResponse result)
        {
            Debug.WriteLine(JsonConvert.SerializeObject(result));
            return Task.CompletedTask;
        }

        public Task ActionAfterResult(HealthyResponse result)
        {
            return Task.CompletedTask;
        }

        public Task ActionAfterSuccess(HealthyResponse result)
        {
            return Task.CompletedTask;
        }
    }
}
