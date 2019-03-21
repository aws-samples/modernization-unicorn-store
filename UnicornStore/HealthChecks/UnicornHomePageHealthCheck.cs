using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnicornStore.HealthChecks
{
    public class UnicornHomePageHealthCheck : IHealthCheck
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnicornHomePageHealthCheck(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string myUrl = request.Scheme + "://" + request.Host.ToString();

            var client = new HttpClient();
            var response = await client.GetAsync(myUrl);
            if (response.IsSuccessStatusCode.Equals(true))
            {
                return HealthCheckResult.Healthy(response.StatusCode.ToString());
            }

            return HealthCheckResult.Unhealthy(response.StatusCode.ToString());
        }
    }
}