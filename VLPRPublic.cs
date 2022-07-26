using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class VehicleInfo
{

    public VehicleInfo()
    {

    }
    internal VehicleInfo(string plate, byte[] imgbuff, byte[] twobuff, string name, long handle)
    {
        VehicleId = plate;
        Image = imgbuff;
        TwoBin = twobuff;
        Name = name;
        Handle = handle;
    }

    /// <summary>
    /// 车牌识别名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 车牌识别句柄
    /// </summary>
    public long Handle { get; }
    /// <summary>
    /// 车牌识别号码 格式为 新A515MG_0 
    /// </summary>
    public string VehicleId { get; set; }
    /// <summary>
    /// 原始大图
    /// </summary>
    public byte[] Image { get; set; }
    /// <summary>
    /// 二值化图像
    /// </summary>
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
    public  List<VLPRConfig> VLPRConfigs { get; set; } = new List<VLPRConfig>();
 
    /// <summary>
    /// 检查车牌识别状态的时间间隔
    /// </summary>
    public double Interval { get; set; } = 120;

    /// <summary>
    /// 接口为建议协议， 不支持多相机
    /// </summary>
    public bool EasyVLPR { get; set; } = false;

    /// <summary>
    /// 车道和摄像机映射关系 用来支持一个摄像机多个车道
    /// </summary>
    public Dictionary<int, string> Lanes { get; set; }= new Dictionary<int, string>();
}

public class VLPRClient  
{
    internal Func<string, bool> HCapture { get;  set; }
    internal Func<string, bool> HCheckStatus { get; set; }
    /// <summary>
    /// 抓拍
    /// </summary>
    /// <param name="name">摄像机名称，这个是VLPRConfig中的名称 </param>
    public bool  Capture(string name)
    {
        return (bool)(HCapture?.Invoke(name));
    }
    public event EventHandler<VehicleInfo> FoundVehicle;
    internal void Vlpr_FoundVehicle(object? sender, VehicleInfo e)
    {
        FoundVehicle?.Invoke(sender, e);
    }
    /// <summary>
    /// 检查状态
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool CheckStatus(string name)
    {
        return (bool)HCheckStatus?.Invoke(name);
    }
}