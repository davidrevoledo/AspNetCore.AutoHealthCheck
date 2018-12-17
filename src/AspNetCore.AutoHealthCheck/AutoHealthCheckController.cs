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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Default controller to implement healt check
    /// </summary>
    [Route("api/autoHealthCheck")]
    [ApiController]
    public sealed class AutoHealthCheckController : ControllerBase
    {
        private readonly IHealthChecker _healthChecker;

        public AutoHealthCheckController(IHealthChecker healthChecker)
        {
            _healthChecker = healthChecker;
        }

        /// <summary>
        ///     Default endpoint to process the health check
        /// </summary>
        /// <returns></returns>
        [AvoidAutoHealtCheck]
        [Route("")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _healthChecker.Check().ConfigureAwait(false);
        }
    }
}