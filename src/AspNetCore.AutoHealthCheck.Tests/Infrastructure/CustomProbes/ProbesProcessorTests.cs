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
    public class ProbesProcessorTests
    {
        [Fact]
        public async Task ProbesProcessor_should_do_nothing_with_empty_probes()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            contextAccessor.Setup(c => c.Context)
                .Returns(new AutoHealthCheckContext());

            var processor = new ProbesProcessor(serviceProvider.Object, contextAccessor.Object);

            // act
            var result = await processor.ExecuteCustomProbes();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ProbesProcessor_should_ignore_if_probes_can_not_be_resolved()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            // service provider will not resolve the type
            serviceProvider.Setup(c => c.GetService(It.IsAny<Type>()))
                .Returns(null);

            var context = new AutoHealthCheckContext();
            context.AddProbe<UnImplementedProbe>();
            contextAccessor.Setup(c => c.Context)
                .Returns(context);

            var processor = new ProbesProcessor(serviceProvider.Object, contextAccessor.Object);

            // act
            var result = await processor.ExecuteCustomProbes();

            // assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ProbesProcessor_should_execute_single_well_probe()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            // service provider will not resolve the type
            serviceProvider.Setup(c => c.GetService(It.IsAny<Type>()))
                .Returns(new WellProbe());

            var context = new AutoHealthCheckContext();
            context.AddProbe<WellProbe>();
            contextAccessor.Setup(c => c.Context)
                .Returns(context);

            var processor = new ProbesProcessor(serviceProvider.Object, contextAccessor.Object);

            // act
            var result = await processor.ExecuteCustomProbes();

            // assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.First().Succeed);
            Assert.Equal("WellProbe", result.First().Name);
        }

        [Fact]
        public async Task ProbesProcessor_should_execute_single_bad_probe()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            // service provider will not resolve the type
            serviceProvider.Setup(c => c.GetService(It.IsAny<Type>()))
                .Returns(new BadProbe());

            var context = new AutoHealthCheckContext();
            context.AddProbe<BadProbe>();
            contextAccessor.Setup(c => c.Context)
                .Returns(context);

            var processor = new ProbesProcessor(serviceProvider.Object, contextAccessor.Object);

            // act
            var result = await processor.ExecuteCustomProbes();

            // assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.False(result.First().Succeed);
            Assert.Equal("BadProbe", result.First().Name);
            Assert.Equal("custom error", result.First().ErrorMessage);
        }

        [Fact]
        public async Task ProbesProcessor_should_execute_probes_in_order()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccessor>();
            var serviceProvider = new Mock<IServiceProvider>();

            // service provider will not resolve the type
            serviceProvider.Setup(c => c.GetService(It.Is<Type>(x => x == typeof(BadProbe))))
                .Returns(new BadProbe());

            serviceProvider.Setup(c => c.GetService(It.Is<Type>(x => x == typeof(WellProbe))))
                .Returns(new WellProbe());

            var context = new AutoHealthCheckContext();
            context.AddProbe<BadProbe>();
            context.AddProbe<WellProbe>();
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
            Assert.Equal("custom error", result.First(s => s.Name == "BadProbe").ErrorMessage);

            // well probe
            Assert.True(result.First(s => s.Name == "WellProbe").Succeed);
            Assert.Null(result.First(s => s.Name == "WellProbe").ErrorMessage);
        }

        private class BadProbe : IProbe
        {
            public string Name => typeof(BadProbe).Name;

            public Task<ProbeResult> Check()
            {
                return Task.FromResult(ProbeResult.Error("custom error"));
            }
        }

        private class WellProbe : IProbe
        {
            public string Name => typeof(WellProbe).Name;

            public Task<ProbeResult> Check()
            {
                return Task.FromResult(ProbeResult.Ok());
            }
        }

        private class UnImplementedProbe : IProbe
        {
            public string Name => typeof(UnImplementedProbe).Name;

            public Task<ProbeResult> Check()
            {
                throw new NotImplementedException();
            }
        }
    }
}
