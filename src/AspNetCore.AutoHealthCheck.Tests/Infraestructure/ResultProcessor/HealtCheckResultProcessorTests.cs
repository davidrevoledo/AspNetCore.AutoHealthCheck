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

using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure.ResultProcessor
{
    public class HealtCheckResultProcessorTests
    {
        [Fact]
        public void HealtCheckResultProcessor_should_return_ok_with_emtpty_results()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();
            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations());

            // act
            var result = HealtCheckResultProcessor.ProcessResult(contex.Object, watch, new HttpResponseMessage[0]);

            // assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = result as JsonResult;

            Assert.NotNull(jsonResult);
            Assert.Equal(200, jsonResult.StatusCode);

            var healthyResponse = jsonResult.Value as HealthyResponse;
            Assert.NotNull(healthyResponse);

            Assert.True(healthyResponse.Success);
        }

        [Fact]
        public void HealtCheckResultProcessor_should_return_ok_with_default_rule()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations()); // default rule

            var messages = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.OK),
                new HttpResponseMessage(HttpStatusCode.AlreadyReported),
                new HttpResponseMessage(HttpStatusCode.Ambiguous),
                new HttpResponseMessage(HttpStatusCode.BadRequest)
            };

            // act
            var result = HealtCheckResultProcessor.ProcessResult(contex.Object, watch, messages.ToArray());

            // assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = result as JsonResult;

            Assert.NotNull(jsonResult);
            Assert.Equal(200, jsonResult.StatusCode);

            var healthyResponse = jsonResult.Value as HealthyResponse;
            Assert.NotNull(healthyResponse);

            Assert.True(healthyResponse.Success);
        }

        [Fact]
        public void HealtCheckResultProcessor_should_return_ok_with_custom_rule()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations
                {
                    PassCheckRule = (m) => m.StatusCode != HttpStatusCode.OK // if is ok should fail
                });

            var messages = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.AlreadyReported),
                new HttpResponseMessage(HttpStatusCode.Ambiguous),
                new HttpResponseMessage(HttpStatusCode.BadRequest),
                new HttpResponseMessage(HttpStatusCode.InternalServerError),
            };

            // act
            var result = HealtCheckResultProcessor.ProcessResult(contex.Object, watch, messages.ToArray());

            // assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = result as JsonResult;

            Assert.NotNull(jsonResult);
            Assert.Equal(200, jsonResult.StatusCode);

            var healthyResponse = jsonResult.Value as HealthyResponse;
            Assert.NotNull(healthyResponse);

            Assert.True(healthyResponse.Success);
        }

        [Fact]
        public void HealtCheckResultProcessor_should_return_custom_response_code_with_default_rule()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations
                {
                    DefaultHealthyResponseCode = HttpStatusCode.Continue
                });

            var messages = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.OK),
                new HttpResponseMessage(HttpStatusCode.AlreadyReported),
                new HttpResponseMessage(HttpStatusCode.Ambiguous),
                new HttpResponseMessage(HttpStatusCode.BadRequest)
            };

            // act
            var result = HealtCheckResultProcessor.ProcessResult(contex.Object, watch, messages.ToArray());

            // assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = result as JsonResult;

            Assert.NotNull(jsonResult);
            Assert.Equal((int)HttpStatusCode.Continue, jsonResult.StatusCode);

            var healthyResponse = jsonResult.Value as HealthyResponse;
            Assert.NotNull(healthyResponse);

            Assert.True(healthyResponse.Success);
        }
    }
}
