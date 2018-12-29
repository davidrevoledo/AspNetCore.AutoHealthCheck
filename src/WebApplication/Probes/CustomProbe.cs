using System.Threading.Tasks;
using AspNetCore.AutoHealthCheck;

namespace WebApplication.Probes
{
    public class CustomProbe : IProbe
    {
        public string Name => typeof(CustomProbe).Name;

        public Task<bool> Check()
        {
            return Task.FromResult(false);
        }
    }
}