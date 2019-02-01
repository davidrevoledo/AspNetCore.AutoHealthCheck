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

namespace AspNetCore.AutoHealthCheck.ApplicationInsights
{
    /// <summary>
    ///     Plugin Configurations for Application insights integration.
    /// </summary>
    public class ApplicationInsightsPluginConfigurations
    {
        private static readonly Lazy<ApplicationInsightsPluginConfigurations> Singleton
            = new Lazy<ApplicationInsightsPluginConfigurations>(() => new ApplicationInsightsPluginConfigurations());

        private ApplicationInsightsPluginConfigurations()
        {
        }

        public static ApplicationInsightsPluginConfigurations Instance => Singleton.Value;

        /// <summary>
        ///     Track Mode.
        /// </summary>
        public TrackMode Mode { get; set; }

        /// <summary>
        ///     Application insights instrumentation key.
        /// </summary>
        public string InstrumentationKey { get; set; }

        /// <summary>
        ///     In case the mode is Availability this indicate the AI name.
        /// </summary>
        public string AvailabilityName { get; set; } = "AspNetCoreAutoHealthCheck";

        /// <summary>
        ///     In case the mode is Event this indicate the AI event name.
        /// </summary>
        public string EventName { get; set; } = "AspNetCoreAutoHealthCheck";
    }
}