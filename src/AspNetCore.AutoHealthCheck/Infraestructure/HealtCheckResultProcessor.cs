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
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.AutoHealthCheck
{
    internal class HealtCheckResultProcessor
    {
        public static IActionResult ProcessResult(
            IAutoHealthCheckContext context,
            Stopwatch watcher,
            HttpResponseMessage[] results)
        {
            var healtyResponse = new HealthyResponse
            {
                ElapsedSecondsTest = watcher.ElapsedMilliseconds / 1000
            };

            // check if all responses are out of error server range
            if (!results.Any() || results.All(r => context.Configurations.PassCheckRule.Invoke(r)))
                healtyResponse.Success = true;
            else
                foreach (var result in results)
                {
                    if (context.Configurations.PassCheckRule.Invoke(result))
                        continue;

                    healtyResponse.Success = false;
                    healtyResponse.UnhealthyEndpoints.Add(new UnhealthyEndpoint
                    {
                        HttpStatusCode = (int)result.StatusCode,
                        Route = result.RequestMessage?.RequestUri.ToString(),
                        HttpVerb = result.RequestMessage?.Method.Method
                    });
                }

            // get status code
            var responseCode = healtyResponse.Success
                ? context.Configurations.DefaultHealthyResponseCode
                : context.Configurations.DefaultUnHealthyResponseCode;

            return new JsonResult(healtyResponse)
            {
                StatusCode = (int)responseCode
            };
        }
    }
}