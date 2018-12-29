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
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infrastructure.Context
{
    public class AutoHealthCheckContextTests
    {
        [Fact]
        public void AutoHealthCheckContext_add_probe_should_start_with_empty_custom_probes()
        {
            // arrange
            var context = new AutoHealthCheckContext();

            // act
            // assert
            Assert.NotNull(context.Probes);
            Assert.Empty(context.Probes);
        }

        [Fact]
        public void AutoHealthCheckContext_should_add_single_probe()
        {
            // arrange
            var context = new AutoHealthCheckContext();

            // act
            context.AddProbe<FakeProbe>();

            // assert
            Assert.Single(context.Probes);
        }

        [Fact]
        public void AutoHealthCheckContext_should_ignore_repeated_probes()
        {
            // arrange
            var context = new AutoHealthCheckContext();

            // act
            context.AddProbe<FakeProbe>();
            context.AddProbe<FakeProbe>();

            // assert
            Assert.Single(context.Probes);
        }

        [Fact]
        public void AutoHealthCheckContext_add_probe_should_fail_with_interfaces()
        {
            // arrange
            var context = new AutoHealthCheckContext();

            // act
            // assert
            Assert.Throws<InvalidOperationException>(() => context.AddProbe<IProbe>());
        }

        [Fact]
        public void AutoHealthCheckContext_add_probe_should_fail_with_abstract_types()
        {
            // arrange
            var context = new AutoHealthCheckContext();

            // act
            // assert
            Assert.Throws<InvalidOperationException>(() => context.AddProbe<AbstractProbe>());
        }

        private class FakeProbe : IProbe
        {
            public string Name => typeof(FakeProbe).Name;

            public Task<ProbeResult> Check()
            {
                return Task.FromResult(new ProbeResult());
            }
        }

        private abstract class AbstractProbe : IProbe
        {
            public abstract string Name { get; }

            public abstract Task<ProbeResult> Check();
        }
    }
}
