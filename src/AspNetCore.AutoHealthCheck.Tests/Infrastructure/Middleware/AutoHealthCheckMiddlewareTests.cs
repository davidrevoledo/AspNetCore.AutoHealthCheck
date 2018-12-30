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

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infrastructure.Middleware
{
    public class AutoHealthCheckMiddlewareTests
    {
        [Fact]
        public async Task AutoHealthCheckMiddleware_should_do_nothing()
        {
            // arrange
            var engine = new Mock<IHealthChecker>();
            var defaultOptions = new AutoHealthCheckMiddlewareOptions();

            var middleware = new AutoHealthCheckMiddleware(
                async innerHttpContext => { },
                engine.Object,
                defaultOptions);

            // act
            await middleware.Invoke(new DefaultHttpContext());

            // assert
            engine.Verify(c => c.Check(), Times.Never);
        }

        [Theory]
        [InlineData("DELETE")]
        [InlineData("POST")]
        [InlineData("PUT")]
        public async Task AutoHealthCheckMiddleware_should_avoid_run_with_invalid_methods(string method)
        {
            // arrange
            var engine = new Mock<IHealthChecker>();
            var defaultOptions = new AutoHealthCheckMiddlewareOptions();

            var middleware = new AutoHealthCheckMiddleware(
                async innerHttpContext => { },
                engine.Object,
                defaultOptions);

            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            request.Setup(r => r.Method)
                .Returns(method);

            context.Setup(c => c.Request)
                .Returns(request.Object);

            context.Setup(c => c.Response)
                .Returns(response.Object);

            // act
            await middleware.Invoke(context.Object);

            // assert
            engine.Verify(c => c.Check(), Times.Never);
        }

        [Fact]
        public async Task AutoHealthCheckMiddleware_should_avoid_run_with_un_match_url()
        {
            // arrange
            var engine = new Mock<IHealthChecker>();
            var defaultOptions = new AutoHealthCheckMiddlewareOptions();

            var middleware = new AutoHealthCheckMiddleware(
                async innerHttpContext => { },
                engine.Object,
                defaultOptions);

            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            request.Setup(r => r.Method)
                .Returns("GET"); // valid http method

            request.Setup(c => c.Path)
                .Returns(new PathString("/api/notvalidendpoint"));

            context.Setup(c => c.Request)
                .Returns(request.Object);

            context.Setup(c => c.Response)
                .Returns(response.Object);

            // act
            await middleware.Invoke(context.Object);

            // assert
            engine.Verify(c => c.Check(), Times.Never);
        }

        [Fact]
        public async Task AutoHealthCheckMiddleware_should_run_engine()
        {
            // arrange
            var engine = new Mock<IHealthChecker>();
            var defaultOptions = new AutoHealthCheckMiddlewareOptions();

            var middleware = new AutoHealthCheckMiddleware(
                async innerHttpContext => { },
                engine.Object,
                defaultOptions);

            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var response = new DefaultHttpResponse(new DefaultHttpContext());

            request.Setup(r => r.Method)
                .Returns("GET"); // valid http method

            request.Setup(c => c.Path)
                .Returns(new PathString("/api/autoHealthCheck")); // default endpoint

            context.Setup(c => c.Request)
                .Returns(request.Object);

            context.Setup(c => c.Response)
                .Returns(response);

            engine.Setup(c => c.Check())
                .Returns(Task.FromResult(new HealthyResponse()));

            // act
            await middleware.Invoke(context.Object);

            // assert
            engine.Verify(c => c.Check(), Times.Once);
        }

        [Fact]
        public async Task AutoHealthCheckMiddleware_should_fail_if_engine_return_null()
        {
            // arrange
            var engine = new Mock<IHealthChecker>();
            var defaultOptions = new AutoHealthCheckMiddlewareOptions();

            var middleware = new AutoHealthCheckMiddleware(
                async innerHttpContext => { },
                engine.Object,
                defaultOptions);

            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            request.Setup(r => r.Method)
                .Returns("GET"); // valid http method

            request.Setup(c => c.Path)
                .Returns(new PathString("/api/autoHealthCheck")); // default endpoint

            context.Setup(c => c.Request)
                .Returns(request.Object);

            context.Setup(c => c.Response)
                .Returns(response.Object);

            // act
            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await middleware.Invoke(context.Object));
        }
    }
}