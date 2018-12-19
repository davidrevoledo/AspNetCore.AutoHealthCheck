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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure
{
    public class HealthCheckerTests
    {
        [Fact]
        public async Task HealthChecker_should_return_ok_if_there_is_no_route()
        {
            // arrange
            var dicover = new Mock<IRouteDiscover>();
            var httpclientFactory = new Mock<IHttpClientFactory>();
            var contextAccesor = new Mock<IAutoHealthCheckContextAccesor>();
            var endpointBuilder = new Mock<IEndpointBuilder>();
            var endpointTranslator = new Mock<IEndpointMessageTranslator>();

            contextAccesor.Setup(c => c.Context)
                .Returns(new AutoHealthCheckContext());

            var checker = new HealthChecker(
                dicover.Object,
                httpclientFactory.Object,
                endpointBuilder.Object,
                contextAccesor.Object,
                endpointTranslator.Object);

            dicover.Setup(c => c.GetAllEndpoints())
                .Returns(() =>
                {
                    IEnumerable<IRouteInformation> enumerable = new List<IRouteInformation>();
                    return Task.FromResult(enumerable);
                });

            // act
            var result = await checker.Check();

            // assert
            Assert.IsType<JsonResult>(result);

            var jsonResult = result as JsonResult;

            Assert.NotNull(jsonResult);
            Assert.Equal(200, jsonResult.StatusCode);

            var healthyResponse = jsonResult.Value as HealthyResponse;
            Assert.NotNull(healthyResponse);

            Assert.True(healthyResponse.Success);
        }
    }
}