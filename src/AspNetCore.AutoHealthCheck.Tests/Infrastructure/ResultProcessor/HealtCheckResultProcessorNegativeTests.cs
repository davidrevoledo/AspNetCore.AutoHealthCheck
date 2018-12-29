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
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infrastructure.ResultProcessor
{
    public class HealtCheckResultProcessorNegativeTests
    {
        [Fact]
        public async Task HealtCheckResultProcessor_should_fail_returning_500_with_default_rule()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations()); // default rule

            var messages = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.InternalServerError),
                new HttpResponseMessage(HttpStatusCode.BadGateway)
            };

            // act
            var result = await HealtCheckResultProcessor.ProcessResult(contex.Object, watch, messages.ToArray());

            // assert
            Assert.NotNull(result);
            Assert.Equal(500, (int)result.HttpStatus);
            Assert.False(result.Success);
            Assert.Equal(2, result.UnhealthyEndpoints.Count);
            Assert.Equal(500, result.UnhealthyEndpoints[0].HttpStatusCode);
            Assert.Equal(502, result.UnhealthyEndpoints[1].HttpStatusCode);
        }

        [Fact]
        public async Task HealtCheckResultProcessor_should_fail_with_custom_rule()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations
                {
                    PassCheckRule = m => m.StatusCode != HttpStatusCode.OK // if is ok should fail
                });

            var messages = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.AlreadyReported),
                new HttpResponseMessage(HttpStatusCode.OK)
            };

            // act
            var result = await HealtCheckResultProcessor.ProcessResult(contex.Object, watch, messages.ToArray());

            // assert
            Assert.NotNull(result);
            Assert.Equal(500, (int) result.HttpStatus);
            Assert.False(result.Success);
            Assert.Single(result.UnhealthyEndpoints);
        }

        [Fact]
        public async Task HealtCheckResultProcessor_should_fail_returning_custom_response_code_with_default_rule()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var contex = new Mock<IAutoHealthCheckContext>();

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations
                {
                    DefaultUnHealthyResponseCode = HttpStatusCode.AlreadyReported
                });

            var messages = new List<HttpResponseMessage>
            {
                new HttpResponseMessage(HttpStatusCode.InternalServerError),
                new HttpResponseMessage(HttpStatusCode.BadGateway)
            };

            // act
            var result = await HealtCheckResultProcessor.ProcessResult(contex.Object, watch, messages.ToArray());

            // assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.AlreadyReported, result.HttpStatus);
            Assert.False(result.Success);
            Assert.Equal(2, result.UnhealthyEndpoints.Count);
            Assert.Equal(500, result.UnhealthyEndpoints[0].HttpStatusCode);
            Assert.Equal(502, result.UnhealthyEndpoints[1].HttpStatusCode);
        }
    }
}
