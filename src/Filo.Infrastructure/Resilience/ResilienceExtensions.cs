using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Filo.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(Action<DelegateResult<HttpResponseMessage>, TimeSpan, int>? onRetry = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            }, (result, timeSpan, retryCount, context) =>
            {
                onRetry?.Invoke(result, timeSpan, retryCount);
            });
    }
}
