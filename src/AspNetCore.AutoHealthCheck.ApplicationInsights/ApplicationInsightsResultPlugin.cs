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

using System.Threading.Tasks;

namespace AspNetCore.AutoHealthCheck.ApplicationInsights
{
    /// <summary>
    ///     Plugin to interface with Application insights to track result of each test check.
    /// </summary>
    public class ApplicationInsightsResultPlugin : IHealthCheckResultPlugin
    {
        private readonly ITelemetryServices _telemetryServices;

        public ApplicationInsightsResultPlugin(ITelemetryServices telemetryServices)
        {
            _telemetryServices = telemetryServices;
        }

        public string Name => "ApplicationInsightsResultPlugin";

        /// <inheritdoc />
        public async Task ActionAfterResult(HealthyResponse result)
        {
            var configurations = ApplicationInsightsPluginConfigurations.Instance;
            switch (configurations.Mode)
            {
                case TrackMode.Availability:
                    await _telemetryServices.TrackAvailability(result).ConfigureAwait(false);
                    break;

                case TrackMode.Event:
                    await _telemetryServices.TrackEvent(result).ConfigureAwait(false);
                    break;
            }
        }

        /// <inheritdoc />
        public Task ActionAfterSuccess(HealthyResponse result)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task ActionAfterFail(HealthyResponse result)
        {
            // only track fails if the mode is exception
            if (ApplicationInsightsPluginConfigurations.Instance.Mode == TrackMode.Exception)
                await _telemetryServices.TrackException(result).ConfigureAwait(false);
        }
    }
}