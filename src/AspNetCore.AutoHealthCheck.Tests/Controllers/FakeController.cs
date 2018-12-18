using System;
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
    }
}
