﻿//MIT License
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infrastructure.ResultProcessor
{
    public class HealthCheckResultProcessorProbesTests
    {
        [Fact]
        public async Task HealthCheckResultProcessor_should_return_ok_with_successfully_probes()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var context = new Mock<IAutoHealthCheckContext>();

            context.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations()); // default rule

            var probes = new List<ProbeResult>
            {
                ProbeResult.Ok(),
                ProbeResult.Ok()
            };

            // act
            var result = await HealthCheckResultProcessor.ProcessResult(
                context.Object,
                watch,
                new HttpResponseMessage[0],
                probes.ToArray());

            // assert
            Assert.NotNull(result);
            Assert.Equal(200, (int)result.HttpStatus);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task HealthCheckResultProcessor_should_fail_with_bad_probes()
        {
            // arrange
            var watch = new Stopwatch();
            watch.Start();
            var context = new Mock<IAutoHealthCheckContext>();

            context.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations()); // default rule

            var probes = new List<ProbeResult>
            {
                ProbeResult.Error("error"),
                ProbeResult.Ok()
            };

            // act
            var result = await HealthCheckResultProcessor.ProcessResult(
                context.Object,
                watch,
                new HttpResponseMessage[0],
                probes.ToArray());

            // assert
            Assert.NotNull(result);
            Assert.Equal(500, (int)result.HttpStatus);
            Assert.Equal("error", result.UnhealthyProbes.First().ErrorMessage);
            Assert.False(result.Success);
        }
    }
}
