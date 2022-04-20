
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal class VLPRService : BackgroundService
{
    private readonly VLPROptions _setting;
    private readonly Dictionary<VLPRConfig, VLPR> _vprs = new Dictionary<VLPRConfig, VLPR>();
    private readonly VehicleQueue queue;

    public VLPRService(IOptions<VLPROptions> options, VehicleQueue queue)
    {
        _setting = options.Value;
        _setting.VLPRConfigs.ForEach(cfg =>
        {
            var vlpr = new VLPR(cfg);
            _vprs.Add(cfg, vlpr);
        });
        this.queue = queue;
        queue._vprs= _vprs;
        queue.HCapture = Capture;
    }
    internal bool Capture(string name)
    {
        return true;
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
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _vprs.ToList().ForEach(item =>
                {
                    var cfg = item.Key;
                    var vpr = item.Value;
                    var status = vpr.CheckStatus();
                });
                Thread.Sleep(TimeSpan.FromSeconds(_setting.Interval < 10 ? 10 : _setting.Interval));
            }
        });
    }

}
 