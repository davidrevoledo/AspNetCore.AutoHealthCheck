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
    public class ProbesProcessorAsyncTests
    {
        public static int Order;

        [Fact]
        public async Task ProbesProcessor_should_execute_probes_simultaneously()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            // service provider will not resolve the type
            serviceProvider.Setup(c => c.GetService(It.Is<Type>(x => x == typeof(BadProbe))))
                .Returns(new BadProbe());

            serviceProvider.Setup(c => c.GetService(It.Is<Type>(x => x == typeof(WellDelayProbe))))
                .Returns(new WellDelayProbe());

            var context = new AutoHealthCheckContext(new AutoHealthCheckConfigurations
            {
                RunCustomProbesAsync = true // run async
            });

            context.AddProbe<WellDelayProbe>(); // first well delay
            context.AddProbe<BadProbe>();

            contextAccessor.Setup(c => c.Context)
                .Returns(context);

            var processor = new ProbesProcessor(serviceProvider.Object, contextAccessor.Object);

            // act
            var result = await processor.ExecuteCustomProbes();

            // assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            // bad probe
            Assert.False(result.First(s => s.Name == "BadProbe").Succeed);
            Assert.Equal("custom error 0", result.First(s => s.Name == "BadProbe").ErrorMessage);

            // well probe
            Assert.True(result.First(s => s.Name == "WellDelayProbe").Succeed);
            Assert.Null(result.First(s => s.Name == "WellDelayProbe").ErrorMessage);
        }

        private class BadProbe : IProbe
        {
            public string Name => typeof(BadProbe).Name;

            public Task<ProbeResult> Check()
            {
                // should be always 0
                return Task.FromResult(ProbeResult.Error($"custom error {Order}"));
            }
        }

        private class WellDelayProbe : IProbe
        {
            public string Name => typeof(WellDelayProbe).Name;

            public async Task<ProbeResult> Check()
            {
                await Task.Delay(1000);
                Order++;

                return ProbeResult.Ok();
            }
        }
    }
}
