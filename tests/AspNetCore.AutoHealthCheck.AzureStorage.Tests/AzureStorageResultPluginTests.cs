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
using Moq;
using Xunit;

namespace AspNetCore.AutoHealthCheck.AzureStorage.Tests
{
    public class AzureStorageResultPluginTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AzureStorageResultPlugin_should_fail_with_out_connection_string(string cs)
        {
            // arrange
            var storageService = new Mock<IStorageService>();
            var plugin = new AzureStorageResultPlugin(storageService.Object);
            HealthCheckAzureStorageConfigurations.Instance.AzureStorageConnectionString = cs;

            // act
            // assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await plugin.ActionAfterResult(new HealthyResponse {Success = false}));
        }

        [Fact]
        public async Task AzureStorageResultPlugin_should_always_save_failed_result_blob()
        {
            // arrange
            var storageService = new Mock<IStorageService>();
            var plugin = new AzureStorageResultPlugin(storageService.Object);
            HealthCheckAzureStorageConfigurations.Instance.AzureStorageConnectionString = "cs";

            // act
            await plugin.ActionAfterResult(new HealthyResponse {Success = false});

            // assert
            storageService.Verify(c => c.PersistBlobResult(It.IsAny<HealthyResponse>()), Times.Once);
        }

        [Fact]
        public async Task AzureStorageResultPlugin_should_avoid_save_blob_if_only_failed_results_needs_be_saved()
        {
            // arrange
            var storageService = new Mock<IStorageService>();
            var plugin = new AzureStorageResultPlugin(storageService.Object);
            HealthCheckAzureStorageConfigurations.Instance.OnlyTrackFailedResults = true;
            HealthCheckAzureStorageConfigurations.Instance.AzureStorageConnectionString = "cs";

            // act
            await plugin.ActionAfterResult(new HealthyResponse {Success = true});

            // assert
            storageService.Verify(c => c.PersistBlobResult(It.IsAny<HealthyResponse>()), Times.Never);
        }

        [Fact]
        public async Task AzureStorageResultPlugin_should_save_successfully_blob_if_it_was_configured()
        {
            // arrange
            var storageService = new Mock<IStorageService>();
            var plugin = new AzureStorageResultPlugin(storageService.Object);
            HealthCheckAzureStorageConfigurations.Instance.OnlyTrackFailedResults = false;
            HealthCheckAzureStorageConfigurations.Instance.AzureStorageConnectionString = "cs";

            // act
            await plugin.ActionAfterResult(new HealthyResponse {Success = true});

            // assert
            storageService.Verify(c => c.PersistBlobResult(It.IsAny<HealthyResponse>()), Times.Once);
        }
    }
}