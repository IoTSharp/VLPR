using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.Options;

public class VLPRHealthCheck : IHealthCheck
{
    private readonly VLPROptions options;
    private readonly VLPRClient client;

    public VLPRHealthCheck(IOptions<VLPROptions> options, VLPRClient client)

    {
        this.options = options.Value;
        this.client = client;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        List<(string name, bool ok)> lst = new List<(string name, bool ok)>();
        options.VLPRConfigs.ForEach(config =>
        {
            var ok = client.CheckStatus(config.Name);
            lst.Add((config.Name, ok));
        });
        if (lst.All(f => !f.ok))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(description: string.Join(";", lst.Select(c => $"车道:{c.name} {(c.ok ? "正常" : "故障")}").ToList())));
        }
        else if (lst.Any(f => !f.ok))
        {
            return Task.FromResult(HealthCheckResult.Degraded(description: string.Join(";", lst.Select(c => $"车道:{c.name} {(c.ok ? "正常" : "故障")}").ToList())));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
