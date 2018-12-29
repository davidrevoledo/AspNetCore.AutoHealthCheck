namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Result to show unhealthy probes.
    /// </summary>
    public class UnhealthyProbe
    {
        /// <summary>
        ///     Probe name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Probe result.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
