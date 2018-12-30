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
using System.Threading.Tasks;

namespace AspNetCore.AutoHealthCheck
{
    /// <inheritdoc />
    public class ProbesProcessor : IProbesProcessor
    {
        private readonly IAutoHealthCheckContextAccessor _autoHealthCheckContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public ProbesProcessor(
            IServiceProvider serviceProvider,
            IAutoHealthCheckContextAccessor autoHealthCheckContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _autoHealthCheckContextAccessor = autoHealthCheckContextAccessor;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProbeResult>> ExecuteCustomProbes()
        {
            var executionResult = new List<ProbeResult>();
            var currentContext = _autoHealthCheckContextAccessor.Context;

            if (!currentContext.Probes.Any())
                return executionResult;

            // if should run async and there is more than 1 then execute as parallel
            if (currentContext.Configurations.RunCustomProbesAsync && currentContext.Probes.Count > 1)
            {
                var probeResults = currentContext.Probes.Select(p => ExecuteProbe(p)).ToArray();

                await Task.WhenAll(probeResults).ConfigureAwait(false);

                executionResult.AddRange(probeResults.Select(r => r.Result));
            }
            // execute one by one
            else
            {
                foreach (var probeType in currentContext.Probes)
                {
                    var probeResult = await ExecuteProbe(probeType).ConfigureAwait(false);
                    executionResult.Add(probeResult);
                }
            }

            return executionResult.Where(e => e != null);
        }

        private async Task<ProbeResult> ExecuteProbe(Type probeType)
        {
            var probe = GetProbe(probeType);
            if (probe == null)
                return null;

            ProbeResult result = null;
            try
            {
                result = await probe.Check().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // if something fail the mark as bad probe with error message 
                result = ProbeResult.Error(e.Message);
            }
            finally
            {
                if (result != null) result.Name = probe.Name;
            }

            return result;
        }

        /// <summary>
        ///     Resolve a probe
        /// </summary>
        /// <param name="probeType"></param>
        /// <returns></returns>
        private IProbe GetProbe(Type probeType)
        {
            var probeCandidate = _serviceProvider.GetService(probeType);

            switch (probeCandidate)
            {
                default:
                    return null;
                case IProbe probe:
                    return probe;
            }
        }
    }
}