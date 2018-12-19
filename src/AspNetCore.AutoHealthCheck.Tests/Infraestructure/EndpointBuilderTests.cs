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
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure
{
    public class EndpointBuilderTests
    {
        [Fact]
        public async Task EndpointBuilder_should_fail_if_route_is_null()
        {
            // arrange
            var context = new Mock<IHttpContextAccessor>();
            var builder = new EndpointBuilder(context.Object);

            context.Setup(c => c.HttpContext)
                .Returns(new DefaultHttpContext());

            // act
            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => builder.CreateFromRoute(null));
        }

        [Fact]
        public async Task EndpointBuilder_should_return_endpoint()
        {
            // arrange
            var context = new Mock<IHttpContextAccessor>();
            var builder = new EndpointBuilder(context.Object);

            context.Setup(c => c.HttpContext)
                .Returns(new DefaultHttpContext());

            // act
            var endpoint = await builder.CreateFromRoute(new RouteInformation());

            // assert
            Assert.IsType<Endpoint>(endpoint);
        }
    }
}