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

using Xunit;

namespace AspNetCore.AutoHealthCheck.Tests.Infraestructure.Endpoints
{
    public class EndpointMessageTranslatorTests
    {
        [Fact]
        public void EndpointMessageTranslator_should_build_get_message()
        {
            // arrange
            var translator = new EndpointMessageTranslator();
            var endpoint = new Endpoint(new RouteInformation
            {
                Path = "/api/get",
                HttpMethod = "GET"

            }, "https://www.microsoft.com");

            // act
            var message = translator.Transform(endpoint);

            // assert
            Assert.NotNull(message);
            Assert.Equal("https://www.microsoft.com/api/get", message.RequestUri.ToString());
            Assert.Equal("GET", message.Method.Method.ToUpper());
        }

        [Fact]
        public void EndpointMessageTranslator_should_heal_path_if_slash_is_missing()
        {
            // arrange
            var translator = new EndpointMessageTranslator();
            var endpoint = new Endpoint(new RouteInformation
            {
                Path = "api/get",
                HttpMethod = "GET"

            }, "https://www.microsoft.com");

            // act
            var message = translator.Transform(endpoint);

            // assert
            Assert.NotNull(message);
            Assert.Equal("https://www.microsoft.com/api/get", message.RequestUri.ToString());
            Assert.Equal("GET", message.Method.Method.ToUpper());
        }

        // todo more tests are needed to check transformation
    }
}
