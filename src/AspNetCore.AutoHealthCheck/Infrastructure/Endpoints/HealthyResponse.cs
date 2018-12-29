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
using System.Net;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Health check result information.
    /// </summary>
    public class HealthyResponse
    {
        /// <summary>
        ///     Indicate if the check was successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Http Status the check has finished.
        /// </summary>
        public HttpStatusCode HttpStatus { get; set; }

        /// <summary>
        ///     Indicate the elapsed time in seconds.
        /// </summary>
        public long ElapsedSecondsTest { get; set; }

        /// <summary>
        ///     Indicate the endpoints who failed.
        /// </summary>
        public List<UnhealthyEndpoint> UnhealthyEndpoints { get; set; } = new List<UnhealthyEndpoint>();

        /// <summary>
        ///     Indicate the probes who failed.
        /// </summary>
        public List<UnhealthyProbe> UnhealthyProbes { get; set; } = new List<UnhealthyProbe>();
    }
}