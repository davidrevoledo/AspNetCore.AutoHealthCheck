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
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.ApplicationInsights.Tests
{
    public class ApplicationInsightsResultPluginTests
    {
        [Fact]
        public async Task AIPlugin_ActionAfterResult_should_call_TrackAvailability_when_the_mode_is()
        {
            // arrange
            var telemetryService = new Mock<ITelemetryServices>();
            var plugin = new ApplicationInsightsResultPlugin(telemetryService.Object);
            ApplicationInsightsPluginConfigurations.Instance.Mode = TrackMode.Availability;

            // act
            await plugin.ActionAfterResult(new HealthyResponse());

            // assert
            telemetryService.Verify(c=> c.TrackAvailability(It.IsAny<HealthyResponse>()), Times.Once);

            telemetryService.Verify(c => c.TrackEvent(It.IsAny<HealthyResponse>()), Times.Never);
            telemetryService.Verify(c => c.TrackException(It.IsAny<HealthyResponse>()), Times.Never);
        }

        [Fact]
        public async Task AIPlugin_ActionAfterResult_should_call_TrackEvent_when_the_mode_is()
        {
            // arrange
            var telemetryService = new Mock<ITelemetryServices>();
            var plugin = new ApplicationInsightsResultPlugin(telemetryService.Object);
            ApplicationInsightsPluginConfigurations.Instance.Mode = TrackMode.Event;

            // act
            await plugin.ActionAfterResult(new HealthyResponse());

            // assert
            telemetryService.Verify(c => c.TrackEvent(It.IsAny<HealthyResponse>()), Times.Once);

            telemetryService.Verify(c => c.TrackAvailability(It.IsAny<HealthyResponse>()), Times.Never);
            telemetryService.Verify(c => c.TrackException(It.IsAny<HealthyResponse>()), Times.Never);
        }

        [Fact]
        public async Task AIPlugin_ActionAfterFail_should_do_nothing_if_mode_is_not_exception()
        {
            // arrange
            var telemetryService = new Mock<ITelemetryServices>();
            var plugin = new ApplicationInsightsResultPlugin(telemetryService.Object);
            ApplicationInsightsPluginConfigurations.Instance.Mode = TrackMode.Event;

            // act
            await plugin.ActionAfterFail(new HealthyResponse());

            // assert
            telemetryService.Verify(c => c.TrackEvent(It.IsAny<HealthyResponse>()), Times.Never);
            telemetryService.Verify(c => c.TrackAvailability(It.IsAny<HealthyResponse>()), Times.Never);
            telemetryService.Verify(c => c.TrackException(It.IsAny<HealthyResponse>()), Times.Never);
        }

        [Fact]
        public async Task AIPlugin_ActionAfterFail_should_call_TrackException_when_mode_is()
        {
            // arrange
            var telemetryService = new Mock<ITelemetryServices>();
            var plugin = new ApplicationInsightsResultPlugin(telemetryService.Object);
            ApplicationInsightsPluginConfigurations.Instance.Mode = TrackMode.Exception;

            // act
            await plugin.ActionAfterFail(new HealthyResponse());

            // assert
            telemetryService.Verify(c => c.TrackException(It.IsAny<HealthyResponse>()), Times.Once);

            telemetryService.Verify(c => c.TrackEvent(It.IsAny<HealthyResponse>()), Times.Never);
            telemetryService.Verify(c => c.TrackAvailability(It.IsAny<HealthyResponse>()), Times.Never);
        }
    }
}
