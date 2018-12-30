using System;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreUsingDiagnostic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return new StatusCodeResult(500);
        }
    }
}