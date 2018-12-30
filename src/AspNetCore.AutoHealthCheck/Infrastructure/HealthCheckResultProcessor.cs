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

using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.AutoHealthCheck
{
    internal class HealthCheckResultProcessor
    {
        public static async Task<HealthyResponse> ProcessResult(
            IAutoHealthCheckContext context,
            Stopwatch watcher,
            HttpResponseMessage[] endpointResults,
            ProbeResult[] probeResults)
        {
            var healthyResponse = new HealthyResponse
            {
                ElapsedSecondsTest = watcher.ElapsedMilliseconds / 1000,
                Success = true  // default
            };

            // check if there is something to evaluate result.
            if (endpointResults.Length != 0 || probeResults.Length != 0)
            {
                foreach (var result in endpointResults)
                {
                    if (context.Configurations.PassCheckRule.Invoke(result))
                        continue;

                    healthyResponse.Success = false;
                    healthyResponse.UnhealthyEndpoints.Add(new UnhealthyEndpoint
                    {
                        HttpStatusCode = (int)result.StatusCode,
                        Route = result.RequestMessage?.RequestUri.ToString(),
                        HttpVerb = result.RequestMessage?.Method.Method
                    });
                }

                foreach (var probe in probeResults)
                {
                    if (probe.Succeed)
                        continue;

                    healthyResponse.Success = false;
                    healthyResponse.UnhealthyProbes.Add(new UnhealthyProbe
                    {
                        Name = probe.Name,
                        ErrorMessage = probe.ErrorMessage,
                        CustomData = probe.CustomData
                    });
                }
            }

            await ProcessResultPlugins(context, healthyResponse).ConfigureAwait(false);

            // get status code
            healthyResponse.HttpStatus = healthyResponse.Success
                ? context.Configurations.DefaultHealthyResponseCode
                : context.Configurations.DefaultUnHealthyResponseCode;

            return healthyResponse;
        }

        private static async Task ProcessResultPlugins(
            IAutoHealthCheckContext context,
            HealthyResponse healthyResponse)
        {
            foreach (var resultPlugin in context.Configurations.ResultPlugins)
            {
                await resultPlugin.ActionAfterResult(healthyResponse).ConfigureAwait(false);

                switch (healthyResponse.Success)
                {
                    case true:
                        await resultPlugin.ActionAfterSuccess(healthyResponse).ConfigureAwait(false);
                        break;

                    case false:
                        await resultPlugin.ActionAfterFail(healthyResponse).ConfigureAwait(false);
                        break;
                }
            }
        }
    }
}