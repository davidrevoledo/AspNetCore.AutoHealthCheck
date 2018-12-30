using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck;

namespace WebApplication.Probes
{
    public class CustomProbe : IProbe
    {
        public string Name => typeof(CustomProbe).Name;

        public Task<ProbeResult> Check()
        {
            return Task.FromResult(ProbeResult.Error("Custom message"));
        }
    }
}