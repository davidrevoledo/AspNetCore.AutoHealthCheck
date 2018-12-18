using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("querystring")]
        public ActionResult<IEnumerable<string>> GetWithQueryStrings([FromQuery] int? a)
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";

        }

        [HttpPost]
        public void Post([FromBody, Required] string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody, Required] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpPut("int/{id}")]
        public void Put(int id, [FromBody, Required] int value)
        {
            throw new Exception();
        }

        [HttpPut("datetime/{id}")]
        public void Put(int id, [FromBody, Required] DateTime value)
        {
        }

        [HttpPut("bruno/{id}")]
        public void Bruno(int id, [FromBody, Required] DateTime value)
        {
        }

        [HttpPut("bruno2/{id}")]
        public IActionResult Bruno2(int id, DateTime value)
        {
            if (id == 0)
                return StatusCode(400);

            throw new Exception();
        }
    }
}
