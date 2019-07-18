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
            CancellationToken cancellationToken = default)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string myUrl = $"{request.Scheme}://{request.Host}";

            HttpResponseMessage response = await new HttpClient().GetAsync(myUrl);
            string status = response.StatusCode.ToString();

            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy(status) : HealthCheckResult.Unhealthy(status);
        }
    }
}