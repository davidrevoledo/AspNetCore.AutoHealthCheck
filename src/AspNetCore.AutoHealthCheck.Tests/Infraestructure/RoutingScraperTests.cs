using System;
using System.Linq;
using AspNetCore.AutoHealthCheck.Tests.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure
{
    public class RoutingScraperTests
    {
        [Fact]
        public void RouteScraper_should_take_route_constraints()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api/{id}/test/{foo}"
            };

            // act
            var constraints = RoutingScraper.GetRouteConstraints(routeInformation).ToList();

            // assert
            Assert.Equal(2, constraints.Count);
            Assert.Equal("id", constraints[0]);
            Assert.Equal("foo", constraints[1]);
        }

        [Fact]
        public void RouteScraper_should_ignore_route_params_if_constratints_are_emtpy()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "/api/getAll"
            };

            var description = new ControllerActionDescriptor();

            // act
            RoutingScraper.CoumpleteRoutingInformation(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.RouteParams);
        }

        [Theory]
        [InlineData("SingleRouteParamInt", "id", typeof(int))]
        [InlineData("SingleRouteParamDate", "id", typeof(DateTime))]
        [InlineData("SingleRouteParamString", "id", typeof(string))]
        public void RouteScraper_should_resolve_descriptors_methods_with_route_constraints(
            string method,
            string name,
            Type type)
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "{id}"
            };

            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod(method)
            };

            // act
            RoutingScraper.CoumpleteRoutingInformation(routeInformation, description);

            // assert
            Assert.Single(routeInformation.RouteParams);
            Assert.Equal(name, routeInformation.RouteParams.First().Key);
            Assert.Equal(type, routeInformation.RouteParams.First().Value);
        }

        [Fact]
        public void RouteScraper_should_ignore_route_constraints_if_they_are_body_or_query_params()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "query/{id}"
            };

            // should avoid FromQueryAttribute or FromBodyAttribute
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("FromQuery")
            };

            // act
            RoutingScraper.CoumpleteRoutingInformation(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.RouteParams);
        }

        [Fact]
        public void RouteScraper_should_ignore_route_constraints_if_they_missing_in_method()
        {
            // arrange
            var routeInformation = new RouteInformation
            {
                RouteTemplate = "{id}"
            };

            // should avoid FromQueryAttribute or FromBodyAttribute
            var description = new ControllerActionDescriptor
            {
                MethodInfo = typeof(FakeController).GetMethod("ParameterLessMethod")
            };

            // act
            RoutingScraper.CoumpleteRoutingInformation(routeInformation, description);

            // assert
            Assert.Empty(routeInformation.RouteParams);
        }
    }
}