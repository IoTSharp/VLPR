using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

 
public class VLPRService : BackgroundService
{
    private readonly VLPROptions _setting;
    private readonly Dictionary<VLPRConfig, IVLPR> _vprs = new Dictionary<VLPRConfig, IVLPR>();
    private readonly VehicleQueue queue;

    public VLPRService(IOptions<VLPROptions> options, VehicleQueue queue)
    {
        _setting = options.Value;
        if (_setting.EasyVLPR)
        {
            _setting.VLPRConfigs.ForEach(cfg =>
            {
                var vlpr = new VLPRSingle(cfg);
                _vprs.Add(cfg, vlpr);
            });
        }
        else
        {
            _setting.VLPRConfigs.ForEach(cfg =>
            {
                var vlpr = new VLPR(cfg);
                _vprs.Add(cfg, vlpr);
            });
        }
        this.queue = queue;
        queue._vprs= _vprs;
        queue.HCapture = Capture;
        queue.HSetQueue = SetQueue;
        queue.HSetEvent = SetEvent;
    }
    internal bool Capture(string name)
    {
        return  _vprs.FirstOrDefault(f => f.Key.Name == name).Value.Capture();
    }
    /// <summary>
    /// 设置使用事件
    /// </summary>
    /// <param name="handler"></param>
    internal void SetEvent(EventHandler<VehicleInfo> handler)
    {
        _vprs.ToList().ForEach(item =>
        {
            item.Value.FoundVehicle += handler;
        });
    }
    /// <summary>
    /// 设置使用队列
    /// </summary>
    internal void SetQueue()
    {
        SetEvent(VPRService_handler);
    }
    private void VPRService_handler(object sender, VehicleInfo e)
    {
        queue.TryAdd(e);
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
 