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

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Diagnostics.Tests
{
    public class AspNetCoreDiagnosticHealthCheckTests
    {
        [Fact]
        public async Task AspNetCoreDiagnosticHealthCheck_should_do_nothing_if_context_is_null()
        {
            // arrange
            var clientFactory = new Mock<IHttpClientFactory>();
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var healthCheck = new AspNetCoreDiagnosticHealthCheck(clientFactory.Object, contextAccessor.Object);

            contextAccessor.Setup(c => c.Context)
                .Returns(default(IAutoHealthCheckContext));

            // act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
        }
    }
}
