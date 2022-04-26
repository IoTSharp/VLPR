using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

public class VLPRService : BackgroundService
{
    private readonly VLPROptions _setting;
    private readonly Dictionary<VLPRConfig, IVLPR> _vprs = new Dictionary<VLPRConfig, IVLPR>();
    private readonly VLPRClient _client;
    private readonly IServiceScope _scope;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<VLPRService> _logger;

    public VLPRService(IOptions<VLPROptions> options, VLPRClient client, IServiceScopeFactory scopeFactor)
    {
        _setting = options.Value;
        _client = client;
        _client.HCapture = Capture;
        _scope = scopeFactor.CreateScope();
         _loggerFactory =  _scope.ServiceProvider.GetService<ILoggerFactory>();
        _logger = _loggerFactory?.CreateLogger<VLPRService>();
        if (_setting.EasyVLPR)
        {
            _setting.VLPRConfigs.ForEach(cfg =>
            {
                var vlpr = new VLPRSingle(cfg,_loggerFactory?.CreateLogger($"{nameof(VLPRSingle)}{cfg.Name}"));
                vlpr.FoundVehicle += _client.Vlpr_FoundVehicle;
                _vprs.Add(cfg, vlpr);
            });
        }
        else
        {
            _setting.VLPRConfigs.ForEach(cfg =>
            {
                var vlpr = new VLPR(cfg, _loggerFactory?.CreateLogger($"{nameof(VLPR)}{cfg.Name}"));
                vlpr.FoundVehicle += _client.Vlpr_FoundVehicle;
                _vprs.Add(cfg, vlpr);
            });
        }
      
    }
  
    private bool Capture(string name)
    {
        bool result=false;
        _logger?.LogInformation($"准备调用抓拍{name}");
        if (_vprs.Any(f => f.Key.Name == name))
        {
            var cmp = _vprs.First(f => f.Key.Name == name).Value;
            if (cmp != null)
            {
                result= cmp.Capture();
                _logger?.LogInformation($"名称为{name}抓拍调用结果{result}");
            }
            else

            {
                _logger?.LogWarning($"名称为{name}的相机值为空");
            }
        }
        else
        {
            _logger?.LogWarning($"没有找到名称为{name}的相机");
        }
        return result;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"开始检查相机，共{_vprs?.Count}个");
                _vprs.ToList().ForEach(item =>
                {
                    var cfg = item.Key;
                    var vpr = item.Value;
                    try
                    {
                        var status = vpr.CheckStatus();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"定时检查结果:{ex.Message}");
                    }
                });
               await Task.Delay (TimeSpan.FromSeconds(_setting.Interval < 10 ? 10 : _setting.Interval));
            }
        });
    }

}
 