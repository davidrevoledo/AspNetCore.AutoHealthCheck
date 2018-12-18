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
using System.Collections.Generic;
using System.Linq;
using AspNetCore.AutoHealthCheck.Tests.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure.Scapers
{
    public class QueryStringScraperTests
    {
        [Fact]
        public void QueryStringScraper_should_ignore_if_attribute_is_saved_as_body_param()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api",
                BodyParams = new Dictionary<string, Type>
                {
                    ["id"] = typeof(int)
                }
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("SingleRouteParamInt")
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.QueryParams);
        }

        [Fact]
        public void QueryStringScraper_should_ignore_if_attribute_is_saved_as_query_param()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api/{id}",
                QueryParams = new Dictionary<string, Type>
                {
                    ["id"] = typeof(string)
                }
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("SingleRouteParamInt")
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Single(routeInformation.QueryParams);
        }

        [Fact]
        public void QueryStringScraper_should_ignore_if_type_is_not_supported()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "UnsuportedQueryString"
            };

            // will not support for get a list as querystring
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("UnsuportedQueryString")
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.QueryParams);
        }

        [Theory]
        [InlineData("SingleRouteParamInt", "id", typeof(int))]
        [InlineData("SingleRouteParamDate", "id", typeof(DateTime))]
        [InlineData("SingleRouteParamString", "id", typeof(string))]
        public void QueryStringScraper_should_support_types(
            string method,
            string name,
            Type type)
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "route"
            };

            // will not support for get a list as querystring
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod(method)
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Single(routeInformation.QueryParams);
            Assert.Equal(name, routeInformation.QueryParams.First().Key);
            Assert.Equal(type, routeInformation.QueryParams.First().Value);
        }

        [Fact]
        public void QueryStringScraper_should_ignore_route_constraints()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api{id}"
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("SingleRouteParamInt")
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.QueryParams);
        }

        [Fact]
        public void QueryStringScraper_should_support_complex_get_params_marked_with_from_query()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "ComplexFilter"
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("ComplexFilter")
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Equal(3, routeInformation.QueryParams.Count); // property count of ComplexFilter
            Assert.Single(routeInformation.QueryParams.Where(r => r.Key == "Name" && r.Value == typeof(string)));
            Assert.Single(routeInformation.QueryParams.Where(r => r.Key == "Id" && r.Value == typeof(int)));
            Assert.Single(routeInformation.QueryParams.Where(r => r.Key == "Date" && r.Value == typeof(DateTime)));
        }

        [Fact]
        public void QueryStringScraper_should_ignore_complex_nested_items_in_a_complex_item()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "NestedComplexFilter"
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("NestedComplexFilter")
            };

            // act
            QueryStringScraper.CompleteQueryStringRequiredParams(routeInformation, description);

            // assert
            Assert.Equal(3, routeInformation.QueryParams.Count); 
        }
    }
}
