using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Net;

/// <summary>
/// 多车牌识别
/// </summary>
internal class VLPR : IDisposable, IVLPR
{
    private string _lib = "libvlpr.so";

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
    private   IntPtr _ipaddress;
    private   IntPtr _username;
    private   IntPtr _password;
    private IntPtr _name;
    public delegate long  _VPR_Init(int uPort, int nHWYPort, IntPtr chDevIp);
    public delegate long  _VPR_InitEx(IntPtr capIpAddress, IntPtr username, IntPtr password, int uPort);
    public delegate long _VPR_Quit(long handle);

    public delegate bool _VPR_Capture(long handle);

    public delegate bool _VPR_GetVehicleInfo(long handle,IntPtr pchPlate, IntPtr iPlateColor, IntPtr piByteBinImagLen, IntPtr pByteBinImage, IntPtr piJpegImageLen, IntPtr pByteJpegImage);

    public delegate bool _VPR_CheckStatus(long handle,IntPtr chVprDevStatus);
    public delegate void VPR_EventHandle(long handle,IntPtr userData);

    public delegate int _VPR_SetEventCallBackFunc(long handle,VPR_EventHandle cb, IntPtr userData);

    private VPR_EventHandle eventHandle = null;
    public VLPR(VLPRConfig setting)
    {
        _lib = setting.Provider;
        _setting = setting;
          _ipaddress = Marshal.StringToCoTaskMemAnsi(_setting.IPAddress);
          _username = Marshal.StringToCoTaskMemAnsi(_setting.UserName);
          _password = Marshal.StringToCoTaskMemAnsi(_setting.Password);
          _name = Marshal.StringToCoTaskMemAnsi(_setting.Name);
        _dllHnd = NativeLibrary.Load(_lib);
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
    public long  Handle { get; private set; }
    public string IPAddress { get => _setting.IPAddress; }
    public bool  Init()
    {
     
        var Handle = VPR_InitEx(_ipaddress, _username, _password, 5000);
        int rest = 0;
        if (Handle > 1)
        {
              rest = VPR_SetEventCallBackFunc(Handle, eventHandle,_name);
        }
        _isinit = Handle > 1 && rest>=1;
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
        bRet = VPR_GetVehicleInfo(Handle,chPlate, iPlateColor, piBinLen, chTwo, piJpegLen, chImage);
        if (bRet == true)
        {

            int jpeglen = Marshal.ReadInt32(piJpegLen);
            int platecolor = Marshal.ReadByte(iPlateColor);
            int binlen = Marshal.ReadInt32(piBinLen);
            byte[] buffer = new byte[10];
            Marshal.Copy(chPlate, buffer, 0, 10);
            var plate = System.Text.Encoding.GetEncoding(936).GetString(buffer);
            byte[] imgbuff = new byte[jpeglen];
            Marshal.Copy(chImage, imgbuff, 0, jpeglen);
            byte[] twobuff = new byte[binlen];
            Marshal.Copy(chImage, imgbuff, 0, jpeglen);
            Marshal.Copy(chTwo, twobuff, 0, binlen);
            FoundVehicle?.Invoke(this, new VehicleInfo($"{plate}_{platecolor}", imgbuff, twobuff,Name,handle));
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
        return VPR_Capture(Handle);
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
                Init();
            }
            check = VPR_CheckStatus(Handle,ptrstatus);
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
        Marshal.FreeCoTaskMem(_ipaddress);
        Marshal.FreeCoTaskMem(_username);
        Marshal.FreeCoTaskMem(_password);
        Marshal.FreeCoTaskMem(_name);
        VPR_Quit(Handle);
        NativeLibrary.UnLoad(_dllHnd);
        FoundVehicle = null;
    }
}