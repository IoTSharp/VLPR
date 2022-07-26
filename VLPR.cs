using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text;

/// <summary>
/// 多车牌识别
/// </summary>
internal class VLPR : IDisposable, IVLPR
{
    private readonly ILogger _logger;
    private readonly string _lib;

    private IntPtr _dllHnd;
#pragma warning disable 0649
    public _VPR_Init VPR_Init;
    public _VPR_InitEx VPR_InitEx;
    public _VPR_Quit VPR_Quit;
    public _VPR_Capture VPR_Capture;
    public _VPR_GetVehicleInfo VPR_GetVehicleInfo;
    public _VPR_CheckStatus VPR_CheckStatus;
    public _VPR_SetEventCallBackFunc VPR_SetEventCallBackFunc;
#pragma warning restore 0649
    private readonly VLPRConfig _setting;
    private readonly IntPtr _ipaddress;
    private readonly IntPtr _username;
    private readonly IntPtr _password;
    private readonly IntPtr _name;
    public delegate long  _VPR_Init(int uPort, int nHWYPort, IntPtr chDevIp);
    public delegate long  _VPR_InitEx(IntPtr capIpAddress, IntPtr username, IntPtr password, int uPort);
    public delegate long _VPR_Quit(long handle);

    public delegate bool _VPR_Capture(long handle);

    public delegate bool _VPR_GetVehicleInfo(long handle,IntPtr pchPlate, IntPtr iPlateColor, IntPtr piByteBinImagLen, IntPtr pByteBinImage, IntPtr piJpegImageLen, IntPtr pByteJpegImage);

    public delegate bool _VPR_CheckStatus(long handle,IntPtr chVprDevStatus);
    public delegate void VPR_EventHandle(long handle,IntPtr userData);

    public delegate int _VPR_SetEventCallBackFunc(long handle,VPR_EventHandle cb, IntPtr userData);

    private VPR_EventHandle eventHandle = null;
    public VLPR(VLPRConfig setting, ILogger logger)
    {
        _logger = logger;
        _lib = setting.Provider;
        _setting = setting;
        _ipaddress = Marshal.StringToHGlobalAnsi(_setting.IPAddress);
        _username = Marshal.StringToHGlobalAnsi(_setting.UserName);
        _password = Marshal.StringToHGlobalAnsi(_setting.Password);
        _name = Marshal.StringToHGlobalAnsi(_setting.Name);
    }

    public string Name { get => _setting.Name; }
    public long  Handle { get; private set; }
    public string IPAddress { get => _setting.IPAddress; }
    public bool  Load()
    {
        _dllHnd = NativeLibrary.Load(_lib);
        if (_dllHnd != IntPtr.Zero)
        {
            NativeLibrary.LinkAllDelegates(this, _dllHnd);
            eventHandle = new VPR_EventHandle(EventHandle);
            _logger?.LogInformation("加载动态库成功");
        }
        else
        {
            _logger?.LogError ($"无法加载{_lib}");
        }
        if (_dllHnd != IntPtr.Zero && VPR_InitEx!=null && VPR_SetEventCallBackFunc!=null && VPR_GetVehicleInfo!=null && VPR_Capture!=null)
        {
            _logger?.LogInformation("开始初始化");
              Handle = VPR_InitEx(_ipaddress, _username, _password, _setting.Port);
            int rest = 0;
            if (Handle > 1)
            {
                _logger?.LogInformation("开始设置回调");
                rest = VPR_SetEventCallBackFunc(Handle, eventHandle, _name);
                _logger?.LogInformation($"设置回调完成{rest}");
            }
            else
            {
                _logger?.LogError($"初始化失败{Handle}");
            }
            _isinit = Handle > 1 && rest >= 1;
        }
        else
        {
            _logger?.LogError($"关键函数未实现");
        }
        return _isinit;
    }


    public event EventHandler<VehicleInfo> FoundVehicle;
    private void EventHandle(long handle, IntPtr userData)
    {
        IntPtr chImage = Marshal.AllocHGlobal(1024 * 1024 * 10);
        IntPtr chTwo = Marshal.AllocHGlobal(1024);
        IntPtr chPlate = Marshal.AllocHGlobal(1024);
        IntPtr piBinLen = Marshal.AllocHGlobal(4);
        IntPtr piJpegLen = Marshal.AllocHGlobal(4);
        IntPtr iPlateColor = Marshal.AllocHGlobal(10);
        bool bRet = false;
        _logger?.LogInformation($"{Name}({Handle}，{handle})收到车牌");
        bRet = VPR_GetVehicleInfo(Handle,chPlate, iPlateColor, piBinLen, chTwo, piJpegLen, chImage);
        if (bRet)
        {
            _logger?.LogInformation($"{Name}({Handle}，{handle})收到车牌{chPlate}");
            int jpeglen = Marshal.ReadInt32(piJpegLen);
            int platecolor = Marshal.ReadByte(iPlateColor);
            int binlen = Marshal.ReadInt32(piBinLen);
            byte[] buffer = new byte[10];
            Marshal.Copy(chPlate, buffer, 0, 10);
            var plate = Encoding.GetEncoding(936).GetString(buffer).RemoveNull();
            byte[] imgbuff = new byte[jpeglen];
            Marshal.Copy(chImage, imgbuff, 0, jpeglen);
            byte[] twobuff = new byte[binlen];
            Marshal.Copy(chImage, imgbuff, 0, jpeglen);
            Marshal.Copy(chTwo, twobuff, 0, binlen);
            Task.Run(() =>
            {
                _logger?.LogInformation($"{Name}({Handle}，{handle})事件触发");
                
                FoundVehicle?.Invoke(this, new VehicleInfo($"{plate}_{platecolor}", imgbuff, twobuff, Name, handle));
            });
            _logger?.LogInformation($"{Name}({Handle}，{handle})事件触发完成");
        }
        else
        {
            _logger?.LogWarning($"收到车牌消息，但是获取车牌失败:{bRet}");
        }
        Marshal.FreeHGlobal(chImage);
        Marshal.FreeHGlobal(chTwo);
        Marshal.FreeHGlobal(chPlate);
        Marshal.FreeHGlobal(piBinLen);
        Marshal.FreeHGlobal(piJpegLen);
        Marshal.FreeHGlobal(iPlateColor);
    }

    public bool Capture()
    {
        _logger?.LogInformation($"{Name}({Handle})开始抓拍");
        return VPR_Capture(Handle);
    }
    bool _isinit = false;

    public bool CheckStatus()
    {
        var check = false;
        _logger?.LogInformation($"{Name}({Handle})开始检查状态");
        IntPtr ptrstatus = Marshal.AllocHGlobal(128);
        if (_dllHnd == IntPtr.Zero || !_isinit)
        {
            _logger?.LogInformation($"{Name}({Handle})未初始化");
            Load();
        }
        if (VPR_CheckStatus != null)
        {
            check = VPR_CheckStatus(Handle, ptrstatus);
            _logger?.LogInformation($"{Name}({Handle})状态{check}");
            if (check == false)
            {
                _isinit = false;
            }
        }
        Marshal.FreeHGlobal(ptrstatus);
        return check;
    }
    public void Unload()
    {
        Dispose();
    }
    public void Dispose()
    {
        Marshal.FreeHGlobal(_ipaddress);
        Marshal.FreeHGlobal(_username);
        Marshal.FreeHGlobal(_password);
        Marshal.FreeHGlobal(_name);
        VPR_Quit(Handle);
        NativeLibrary.UnLoad(_dllHnd);
        FoundVehicle = null;
    }
}