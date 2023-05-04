
using System;
using System.Reflection;
using System.Runtime.InteropServices;
internal static class NativeLibrary
{
    #region DllImport
    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string filename);

    [DllImport("kernel32.dll")]
    private static extern bool FreeLibrary(IntPtr hModule);


    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

    [DllImport("libdl.so")]
    private static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.so")]
    private static extern void dlclose(IntPtr mHnd);

    [DllImport("libdl.so")]
    private static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport("libdl.so")]
    private static extern IntPtr dlerror();

    const int RTLD_NOW = 2;
    #endregion

    #region Abstracted
    public static bool __linux__
    {
        get
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
    #endregion

    #region Fields
    private static Type _delegateType = typeof(MulticastDelegate);
    #endregion

    #region Methods

    public static string Error()
    {
        string _error = "unknow";
        if (__linux__)
        {
            IntPtr errPtr = dlerror();
            if (errPtr != IntPtr.Zero)
            {
                _error = Marshal.PtrToStringAnsi(errPtr);
            }
            else
            {
                _error = "";
            }
        }
        else
        {
            _error = "not linux.";
        }
        return _error;
    }
 
    public static IntPtr Load(string filename)
    {
        IntPtr mHnd;

        if (__linux__)
        {
            dlerror();      // clear previous errors if any
            mHnd = dlopen(filename, RTLD_NOW);
        }
        else
            mHnd = LoadLibrary(filename);

        return mHnd;
    }
    public static void UnLoad(IntPtr mHnd)
    {

        if (__linux__)
            dlclose(mHnd);
        else
            FreeLibrary(mHnd);
    }
    public static IntPtr Symbol(IntPtr mHnd, string symbol)
    {
        IntPtr symPtr;

        if (__linux__)
            symPtr = dlsym(mHnd, symbol);
        else
            symPtr = GetProcAddress(mHnd, symbol);

        return symPtr;
    }

    public static Delegate Delegate(Type delegateType, IntPtr mHnd, string symbol)
    {
        IntPtr ptrSym = Symbol(mHnd, symbol);
        return Marshal.GetDelegateForFunctionPointer(ptrSym, delegateType);
    }

    public static void LinkAllDelegates<T>(T obj, IntPtr mHnd)
    {
        FieldInfo[] fields = typeof(T).GetFields();

        foreach (FieldInfo fi in fields)
        {
            if (fi.FieldType.BaseType == _delegateType)
            {
                fi.SetValue(obj, Marshal.GetDelegateForFunctionPointer(Symbol(mHnd, fi.Name), fi.FieldType));
            }
        }
    }
    #endregion
}

