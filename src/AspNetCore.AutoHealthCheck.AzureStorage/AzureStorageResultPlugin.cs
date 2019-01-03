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
using System.Threading.Tasks;

namespace AspNetCore.AutoHealthCheck.AzureStorage
{
    /// <summary>
    ///     Azure Storage Result Plugin to save json results in an Azure Storage Account.
    /// </summary>
    public class AzureStorageResultPlugin : IHealthCheckResultPlugin
    {
        private readonly IStorageService _storageService;

        public AzureStorageResultPlugin(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public string Name => "AzureStorageResultPlugin";

        /// <inheritdoc />
        public async Task ActionAfterResult(HealthyResponse result)
        {
            var configurations = HealthCheckAzureStorageConfigurations.Instance;

            // avoid calling if only failed results needs to be tracked and the was successfully.
            if (configurations.OnlyTrackFailedResults && result.Success)
                return;

            if (string.IsNullOrWhiteSpace(configurations.AzureStorageConnectionString))
                throw new InvalidOperationException(
                    "Please configure the azure storage connection string Property Name : AzureStorageConnectionString");

            await _storageService.PersistBlobResult(result).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task ActionAfterSuccess(HealthyResponse result)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ActionAfterFail(HealthyResponse result)
        {
            return Task.CompletedTask;
        }
    }
}