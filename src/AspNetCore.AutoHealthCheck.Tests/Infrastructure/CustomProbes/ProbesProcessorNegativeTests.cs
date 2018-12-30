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
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infrastructure.CustomProbes
{
    public class ProbesProcessorNegativeTests
    {
        public async Task ProbesProcessor_should_not_fail_with_unhandled_probe()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            // service provider will not resolve the type
            serviceProvider.Setup(c => c.GetService(It.Is<Type>(x => x == typeof(UnImplementedProbe))))
                .Returns(new UnImplementedProbe());

            var context = new AutoHealthCheckContext();
            context.AddProbe<UnImplementedProbe>(); // first well delay

            contextAccessor.Setup(c => c.Context)
                .Returns(context);

            var processor = new ProbesProcessor(serviceProvider.Object, contextAccessor.Object);

            // act
            var result = await processor.ExecuteCustomProbes();

            // assert
            Assert.NotNull(result);
            Assert.Single(result);

            Assert.False(result.First().Succeed);
            Assert.Equal("foo", result.First().ErrorMessage);
        }

        private class UnImplementedProbe : IProbe
        {
            public string Name => typeof(UnImplementedProbe).Name;

            public Task<ProbeResult> Check()
            {
                throw new NotImplementedException("foo");
            }
        }
    }
}
