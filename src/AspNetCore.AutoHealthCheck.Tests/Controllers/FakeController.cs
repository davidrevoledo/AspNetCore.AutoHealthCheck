using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.AutoHealthCheck.Tests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FakeController : ControllerBase
    {
        [HttpDelete("{id}")]
        public void SingleRouteParamInt(int id)
        {
        }

        [HttpDelete("{id}")]
        public void SingleRouteParamDate(DateTime id)
        {
        }

        [HttpDelete("{id}")]
        public void SingleRouteParamString(string id)
        {
        }

        [HttpDelete("")]
        public void ParameterLessMethod()
        {
        }

        [HttpPost("query/{id}/{foo}")]
        public void FromQuery([FromQuery] string id, [FromBody] string foo)
        {
        }

        [HttpPost("doubleFromBody")]
        public void DoubleFromBody([FromBody] int id, [FromBody] string foo)
        {
        }

        [HttpPost("postWithoutFromBody")]
        public void PostWithOutFromBody(string foo)
        {
        }

        [HttpPost("PostWithDuplicatedRouteAndBodyParam/{foo}")]
        public void PostWithDuplicatedRouteAndBodyParam([FromBody] string foo)
        {
        }

        [HttpGet("UnsuportedQueryString")]
        public void UnsuportedQueryString(List<string> ids)
        {
        }

        [HttpGet("ComplexFilter")]
        public void ComplexFilter([FromQuery] ComplexFilter filter)
        {
        }

        [HttpGet("NestedComplexFilter")]
        public void NestedComplexFilter([FromQuery] NestedComplexFilter filter)
        {
        }
    }
}
