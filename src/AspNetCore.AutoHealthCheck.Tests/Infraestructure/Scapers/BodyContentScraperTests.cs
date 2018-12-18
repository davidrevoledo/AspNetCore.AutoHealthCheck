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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspNetCore.AutoHealthCheck.Tests.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure.Scapers
{
    public class BodyContentScraperTests
    {
        [Theory]
        [ClassData(typeof(RouteTestData))]
        public void BodyContentScraper_should_ignore_non_body_requests_type(IRouteInformation routeInformation)
        {
            // ignore get and delete

            // act
            BodyContentScraper.CompleteBodyRequiredContent(routeInformation, new ControllerActionDescriptor());

            // assert
            Assert.Empty(routeInformation.BodyParams);
        }

        [Fact]
        public void BodyContentScraper_should_ignore_methods_without_params()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api/{id}/test/{foo}"
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("ParameterLessMethod")
            };

            // act
            BodyContentScraper.CompleteBodyRequiredContent(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.BodyParams);
        }

        [Fact]
        public void BodyContentScraper_should_take_first_from_body_attribute()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "DoubleFromBody",
                HttpMethod = "POST"
            };

            // this will not run in asp.net core but just to be sure the package support 1 body param
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("DoubleFromBody")
            };

            // act
            BodyContentScraper.CompleteBodyRequiredContent(routeInformation, description);

            // assert
            Assert.Single(routeInformation.BodyParams);
            Assert.Equal("id", routeInformation.BodyParams.First().Key);
            Assert.Equal(typeof(int), routeInformation.BodyParams.First().Value);
        }

        [Fact]
        public void BodyContentScraper_should_consider_params_with_out_from_body_attribute()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api/{id}/test",
                HttpMethod = "POST"
            };

            // this will not run in asp.net core but just to be sure the package support 1 body param
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("PostWithOutFromBody")
            };

            // act
            BodyContentScraper.CompleteBodyRequiredContent(routeInformation, description);

            // assert
            Assert.Single(routeInformation.BodyParams);
            Assert.Equal("foo", routeInformation.BodyParams.First().Key);
            Assert.Equal(typeof(string), routeInformation.BodyParams.First().Value);
        }

        [Fact]
        public void BodyContentScraper_should_ignore_params_if_they_are_included_as_routing_constraints()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api/{foo}/test",
                HttpMethod = "POST"
            };

            // this will not run in asp.net core but just to be sure the package support 1 body param
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("PostWithDuplicatedRouteAndBodyParam")
            };

            // act
            BodyContentScraper.CompleteBodyRequiredContent(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.BodyParams);
        }

        private class RouteTestData : IEnumerable<object[]>
        {
            // arrange
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new RouteInformation
                    {
                        RouteTemplate = "/api/{id}/test/{foo}",
                        HttpMethod = "GET"
                    }
                };

                yield return new object[]
                {
                    new RouteInformation
                    {
                        RouteTemplate = "/api/{id}/test/{foo}",
                        HttpMethod = "DELETE"
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
