using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Text;

/// <summary>
/// 单实例车牌识别
/// </summary>
internal class VLPRSingle : IDisposable, IVLPR
{
    private string _lib = "libvlpr.so";
    private IntPtr _dllHnd;
    private readonly ILogger _logger;
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


    public delegate bool _VPR_Init(int uPort, int nHWYPort, IntPtr chDevIp);
    public delegate bool _VPR_InitEx(IntPtr capIpAddress, IntPtr username, IntPtr password, int uPort);
    public delegate bool _VPR_Quit();

    public delegate bool _VPR_Capture();

    public delegate bool _VPR_GetVehicleInfo(IntPtr pchPlate, IntPtr iPlateColor, IntPtr piByteBinImagLen, IntPtr pByteBinImage, IntPtr piJpegImageLen, IntPtr pByteJpegImage);

    public delegate bool _VPR_CheckStatus(IntPtr chVprDevStatus);
    public delegate void VPR_EventHandle();

    public delegate int _VPR_SetEventCallBackFunc(VPR_EventHandle cb);

    private VPR_EventHandle eventHandle = null;
    public VLPRSingle(VLPRConfig setting, ILogger logger)
    {
        _lib = setting.Provider;
        _setting = setting;
        _dllHnd = NativeLibrary.Load(_lib);
        _logger = logger;
        if (_dllHnd != IntPtr.Zero)
        {
            NativeLibrary.LinkAllDelegates(this, _dllHnd);
            eventHandle = new VPR_EventHandle(EventHandle);
        }
        else
        {
            throw new Exception($"无法加载{setting.Provider}");
        }

    }

    public string Name { get => _setting.Name; }
    public string IPAddress { get => _setting.IPAddress; }
    public bool Load()
    {
        IntPtr _ipaddress = Marshal.StringToCoTaskMemAnsi(_setting.IPAddress);
        IntPtr _username = Marshal.StringToCoTaskMemAnsi(_setting.UserName);
        IntPtr _password = Marshal.StringToCoTaskMemAnsi(_setting.Password);
        var init = VPR_InitEx(_ipaddress, _username, _password, 5000);
        if (init)
        {
            int rest = VPR_SetEventCallBackFunc(eventHandle);
        }
        Marshal.FreeCoTaskMem(_ipaddress);
        Marshal.FreeCoTaskMem(_username);
        Marshal.FreeCoTaskMem(_password);
        _isinit = init;
        return init;
    }


    public event EventHandler<VehicleInfo> FoundVehicle;
    public void EventHandle()
    {
        IntPtr chImage = Marshal.AllocHGlobal(1024 * 1024 * 2);
        IntPtr chTwo = Marshal.AllocHGlobal(128);
        IntPtr chPlate = Marshal.AllocHGlobal(64);
        IntPtr piBinLen = Marshal.AllocHGlobal(4);
        IntPtr piJpegLen = Marshal.AllocHGlobal(4);
        IntPtr iPlateColor = Marshal.AllocHGlobal(10);
        bool bRet = false;
        bRet = VPR_GetVehicleInfo(chPlate, iPlateColor, piBinLen, chTwo, piJpegLen, chImage);
        if (bRet == true)
        {

            int jpeglen = Marshal.ReadInt32(piJpegLen);
            int platecolor = Marshal.ReadByte(iPlateColor);
            int binlen = Marshal.ReadInt32(piBinLen);
            byte[] buffer = new byte[10];
            Marshal.Copy(chPlate, buffer, 0, 10);
            var plate = System.Text.Encoding.GetEncoding(936).GetString(buffer).RemoveNull();
            byte[] imgbuff = new byte[jpeglen];
            Marshal.Copy(chImage, imgbuff, 0, jpeglen);
            byte[] twobuff = new byte[binlen];
            Marshal.Copy(chImage, imgbuff, 0, jpeglen);
            Marshal.Copy(chTwo, twobuff, 0, binlen);
            FoundVehicle?.Invoke(this, new VehicleInfo($"{plate}_{platecolor}", imgbuff, twobuff,Name, Environment.CurrentManagedThreadId) );
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
        return VPR_Capture();
    }
    bool _isinit = false;

    public bool CheckStatus()
    {
        var check = false;
        if (_dllHnd != IntPtr.Zero && VPR_CheckStatus != null && VPR_InitEx != null)
        {
            IntPtr ptrstatus = Marshal.AllocHGlobal(128);
            if (_isinit == false)
            {
                Load();
            }
            check = VPR_CheckStatus(ptrstatus);
            if (check == false)
            {
                _isinit = false;
            }
            Marshal.FreeHGlobal(ptrstatus);
        }
        return check;
    }

    public void Dispose()
    {
        NativeLibrary.UnLoad(_dllHnd);
        FoundVehicle = null;
        VPR_Quit();
    }
}