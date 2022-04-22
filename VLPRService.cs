using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
public class VLPRService : BackgroundService
{
    private readonly VLPROptions _setting;
    private readonly Dictionary<VLPRConfig, IVLPR> _vprs = new Dictionary<VLPRConfig, IVLPR>();
    private readonly VLPRClient _client;

    public VLPRService(IOptions<VLPROptions> options, VLPRClient client)
    {
        _setting = options.Value;
        if (_setting.EasyVLPR)
        {
            _setting.VLPRConfigs.ForEach(cfg =>
            {
                var vlpr = new VLPRSingle(cfg);
                vlpr.FoundVehicle += _client.Vlpr_FoundVehicle;
                _vprs.Add(cfg, vlpr);
            });
        }
        else
        {
            _setting.VLPRConfigs.ForEach(cfg =>
            {
                var vlpr = new VLPR(cfg);
                vlpr.FoundVehicle += _client.Vlpr_FoundVehicle;
                _vprs.Add(cfg, vlpr);
            });
        }
        _client = client;
        _client.HCapture = Capture;
    }
  
    private bool Capture(string name)
    {
        return  _vprs.FirstOrDefault(f => f.Key.Name == name).Value.Capture();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _vprs.ToList().ForEach(item =>
                {
                    var cfg = item.Key;
                    var vpr = item.Value;
                    var status = vpr.CheckStatus();
                });
               await Task.Delay (TimeSpan.FromSeconds(_setting.Interval < 10 ? 10 : _setting.Interval));
            }
        });
    }

}
 