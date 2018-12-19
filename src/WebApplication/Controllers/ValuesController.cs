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
        }

        [HttpPut("datetime/{id}")]
        public void Put(int id, [FromBody, Required] DateTime value)
        {
        }

        [HttpGet("complexGet/{id}")]
        public IActionResult ComplexGet(int id, [FromQuery] ComplexGetParam param)
        {
            throw new Exception();
        }
    }

    public class ComplexGetParam
    {
        public DateTime DateTime { get; set; }

        public string Value { get; set; }

        public int Number { get; set; }
    }
}
