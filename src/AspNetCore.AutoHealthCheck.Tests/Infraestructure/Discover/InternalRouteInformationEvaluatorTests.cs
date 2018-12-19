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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck.Extensibility;
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure.Discover
{
    public class InternalRouteInformationEvaluatorTests
    {
        [Fact]
        public async Task InternalRouteInformationEvaluator_should_ignore_route_if_any_regex_is_excluding_template()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccesor>();
            var contex = new Mock<IAutoHealthCheckContext>();
            var routeEvaluator = new Mock<IRouteEvaluator>();

            routeEvaluator.Setup(c => c.Evaluate(It.IsAny<IRouteInformation>()))
                .Returns(Task.FromResult(true));

            contextAccessor.Setup(c => c.Context)
                .Returns(contex.Object);

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations
                {
                    ExcludeRouteRegexs = new List<Regex>
                    {
                        new Regex("^[0-9]+$")
                    }
                });

            var evaluator = new InternalRouteInformationEvaluator(contextAccessor.Object, routeEvaluator.Object);

            var routeInformation = new RouteInformation
            {
                RouteTemplate = "324234"
            };

            // act
            var include = await evaluator.Evaluate(routeInformation);

            // assert
            Assert.False(include); // route should be ingored as there is numbers
        }

        [Fact]
        public async Task InternalRouteInformationEvaluator_should_not_ignore_route_if_regex_does_not_match()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccesor>();
            var contex = new Mock<IAutoHealthCheckContext>();
            var routeEvaluator = new Mock<IRouteEvaluator>();

            routeEvaluator.Setup(c => c.Evaluate(It.IsAny<IRouteInformation>()))
                .Returns(Task.FromResult(true));

            contextAccessor.Setup(c => c.Context)
                .Returns(contex.Object);

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations
                {
                    ExcludeRouteRegexs = new List<Regex>
                    {
                        new Regex("^[0-9]+$")
                    }
                });

            var evaluator = new InternalRouteInformationEvaluator(contextAccessor.Object, routeEvaluator.Object);

            var routeInformation = new RouteInformation
            {
                RouteTemplate = "get/api/3123" // no only numbers
            };

            // act
            var include = await evaluator.Evaluate(routeInformation);

            // assert
            Assert.True(include); // this should be true as the regex does not match with the route
        }

        [Fact]
        public async Task InternalRouteInformationEvaluator_should_ignore_route_if_evaluator_return_false()
        {
            // arrange
            var contextAccessor = new Mock<IAutoHealthCheckContextAccesor>();
            var contex = new Mock<IAutoHealthCheckContext>();
            var routeEvaluator = new Mock<IRouteEvaluator>();

            routeEvaluator.Setup(c => c.Evaluate(It.IsAny<IRouteInformation>()))
                .Returns(Task.FromResult(false));

            contextAccessor.Setup(c => c.Context)
                .Returns(contex.Object);

            contex.Setup(c => c.Configurations)
                .Returns(new AutoHealthCheckConfigurations());

            var evaluator = new InternalRouteInformationEvaluator(contextAccessor.Object, routeEvaluator.Object);

            var routeInformation = new RouteInformation
            {
                RouteTemplate = "get/api/3123"
            };

            // act
            var include = await evaluator.Evaluate(routeInformation);

            // assert
            Assert.False(include);
        }
    }
}
