using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class VehicleInfo
{

    public VehicleInfo()
    {

    }
    public VehicleInfo(string plate, byte[] imgbuff, byte[] twobuff)
    {
        VehicleId = plate;
        Image = imgbuff;
        TwoBin = twobuff;
    }
    public string VLPRName { get; set; }
    public string VehicleId { get; set; }
    public byte[] Image { get; set; }
    public byte[] TwoBin { get; set; }
}
public class VLPRConfig
{
    public string Provider { get; set; }
    public string Port { get; set; }
    public string IPAddress { get; set; }
    public string Password { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
}


public class VLPROptions
{
    /// <summary>
    /// 车牌识别配置列表
    /// </summary>
    public  List<VLPRConfig> VLPRConfigs { get; set; }
    /// <summary>
    /// 检查车牌识别状态的时间间隔
    /// </summary>
    public double Interval { get;  set; }
}

public class VehicleQueue : BlockingCollection<VehicleInfo>
{
    internal Dictionary<VLPRConfig, VLPR> _vprs;

    public VehicleQueue() : base(new ConcurrentQueue<VehicleInfo>())
    {

    }

    internal Func<string, bool> HCapture { get;  set; }
    /// <summary>
    /// 抓拍
    /// </summary>
    /// <param name="name">摄像机名称，这个是VLPRConfig中的名称 </param>
    public bool  Capture(string name)
    {
        return (bool)(HCapture?.Invoke(name));
    }
}