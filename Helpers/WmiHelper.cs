using System;
using System.Runtime.InteropServices;

namespace DuskControl.Helpers;

internal static partial class WmiHelper
{
  [LibraryImport("ole32.dll")]
  private static partial int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

  [LibraryImport("ole32.dll")]
  private static partial int CoCreateInstance(in Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, in Guid riid, out IntPtr ppv);

  [LibraryImport("oleaut32.dll")]
  private static partial void VariantClear(ref VARIANT pvarg);

  [StructLayout(LayoutKind.Explicit)]
  private struct VARIANT
  {
    [FieldOffset(0)] public ushort vt;
    [FieldOffset(8)] public byte bVal;
    [FieldOffset(8)] public uint ulVal;
    [FieldOffset(8)] public int lVal;
    [FieldOffset(8)] public IntPtr bstrVal;
  }

  private const int RPC_C_AUTHN_LEVEL_DEFAULT = 0;
  private const int RPC_C_IMP_LEVEL_IMPERSONATE = 3;
  private const int EOAC_NONE = 0;

  [LibraryImport("ole32.dll")]
  private static partial int CoInitializeSecurity(
      IntPtr pSecDesc, int cAuthSvc, IntPtr asAuthSvc, IntPtr pReserved1,
      int dwAuthnLevel, int dwImpLevel, IntPtr pAuthList, int dwCapabilities, IntPtr pReserved3);

  [LibraryImport("ole32.dll")]
  private static partial int CoSetProxyBlanket(
      IntPtr pProxy, uint dwAuthnSvc, uint dwAuthzSvc, IntPtr pServerPrincName,
      uint dwAuthnLevel, uint dwImpLevel, IntPtr pAuthInfo, uint dwCapabilities);

  [ThreadStatic]
  private static IntPtr _pSvc;

  public static unsafe bool Initialize()
  {
    if (_pSvc != IntPtr.Zero) return true;

    _ = CoInitializeEx(IntPtr.Zero, 0);
    int hrSec = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RPC_C_AUTHN_LEVEL_DEFAULT, RPC_C_IMP_LEVEL_IMPERSONATE, IntPtr.Zero, EOAC_NONE, IntPtr.Zero);
    if (hrSec < 0 && hrSec != unchecked((int)0x80010119)) return false; // Ignore RPC_E_TOO_LATE

    Guid clsid_WbemLocator = new("4590f811-1d3a-11d0-891f-00aa004b2e24");
    Guid iid_IWbemLocator = new("dc12a687-737f-11cf-884d-00aa004b2e24");

    int hr = CoCreateInstance(in clsid_WbemLocator, IntPtr.Zero, 1, in iid_IWbemLocator, out IntPtr pLoc);
    if (hr < 0 || pLoc == IntPtr.Zero) return false;

    IntPtr bstrResource = Marshal.StringToBSTR(@"ROOT\WMI");
    IntPtr pSvc = IntPtr.Zero;

    IntPtr* vtableLoc = *(IntPtr**)pLoc;
    var connectServer = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr, IntPtr*, int>)vtableLoc[3];
    hr = connectServer(pLoc, bstrResource, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, &pSvc);

    Marshal.FreeBSTR(bstrResource);
    ReleaseComObject(pLoc);

    if (hr < 0 || pSvc == IntPtr.Zero) return false;

    hr = CoSetProxyBlanket(pSvc, 10 /* RPC_C_AUTHN_WINNT */, 0 /* RPC_C_AUTHZ_NONE */, IntPtr.Zero, 3 /* RPC_C_AUTHN_LEVEL_CALL */, 3 /* RPC_C_IMP_LEVEL_IMPERSONATE */, IntPtr.Zero, 0 /* EOAC_NONE */);
    if (hr < 0)
    {
      ReleaseComObject(pSvc);
      return false;
    }

    _pSvc = pSvc;
    return true;
  }

  public static unsafe uint? GetBrightness()
  {
    if (!Initialize()) return null;

    IntPtr bstrQueryLang = Marshal.StringToBSTR("WQL");
    IntPtr bstrQuery = Marshal.StringToBSTR("SELECT * FROM WmiMonitorBrightness");
    IntPtr pEnum = IntPtr.Zero;

    IntPtr* vtableSvc = *(IntPtr**)_pSvc;
    var execQuery = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr*, int>)vtableSvc[20];
    int hr = execQuery(_pSvc, bstrQueryLang, bstrQuery, 0x20 | 0x10, IntPtr.Zero, &pEnum);

    Marshal.FreeBSTR(bstrQueryLang);
    Marshal.FreeBSTR(bstrQuery);

    if (hr < 0 || pEnum == IntPtr.Zero) return null;

    IntPtr* vtableEnum = *(IntPtr**)pEnum;
    var next = (delegate* unmanaged[Stdcall]<IntPtr, int, uint, IntPtr*, uint*, int>)vtableEnum[4];

    IntPtr pObj = IntPtr.Zero;
    uint uReturned = 0;
    hr = next(pEnum, -1, 1, &pObj, &uReturned);
    ReleaseComObject(pEnum);

    if (hr < 0 || uReturned == 0 || pObj == IntPtr.Zero) return null;

    IntPtr bstrPropName = Marshal.StringToBSTR("CurrentBrightness");
    VARIANT varProp = new();

    IntPtr* vtableObj = *(IntPtr**)pObj;
    var get = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, VARIANT*, int*, int*, int>)vtableObj[4];
    hr = get(pObj, bstrPropName, 0, &varProp, null, null);

    Marshal.FreeBSTR(bstrPropName);
    ReleaseComObject(pObj);

    if (hr < 0) return null;

    uint? brightness = null;
    if (varProp.vt == 17 /* VT_UI1 */) brightness = varProp.bVal;

    VariantClear(ref varProp);
    return brightness;
  }

  public static unsafe void SetBrightness(uint brightness)
  {
    if (!Initialize()) return;

    IntPtr bstrClassPath = Marshal.StringToBSTR("WmiMonitorBrightnessMethods");
    IntPtr pClass = IntPtr.Zero;

    IntPtr* vtableSvc = *(IntPtr**)_pSvc;
    var getObject = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr, IntPtr*, IntPtr*, int>)vtableSvc[6];
    int hr = getObject(_pSvc, bstrClassPath, 0, IntPtr.Zero, &pClass, null);

    if (hr < 0 || pClass == IntPtr.Zero)
    {
      Marshal.FreeBSTR(bstrClassPath);
      return;
    }

    IntPtr bstrMethodName = Marshal.StringToBSTR("WmiSetBrightness");
    IntPtr pInSignature = IntPtr.Zero;

    IntPtr* vtableClass = *(IntPtr**)pClass;
    var getMethod = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, IntPtr*, IntPtr*, int>)vtableClass[19]; // Index 19 is GetMethod
    hr = getMethod(pClass, bstrMethodName, 0, &pInSignature, null);
    ReleaseComObject(pClass);

    if (hr < 0 || pInSignature == IntPtr.Zero)
    {
      Marshal.FreeBSTR(bstrClassPath);
      Marshal.FreeBSTR(bstrMethodName);
      return;
    }

    IntPtr pInParams = IntPtr.Zero;
    IntPtr* vtableInSig = *(IntPtr**)pInSignature;
    var spawnInstance = (delegate* unmanaged[Stdcall]<IntPtr, int, IntPtr*, int>)vtableInSig[15]; // Index 15 is SpawnInstance
    hr = spawnInstance(pInSignature, 0, &pInParams);
    ReleaseComObject(pInSignature);

    if (hr < 0 || pInParams == IntPtr.Zero)
    {
      Marshal.FreeBSTR(bstrClassPath);
      Marshal.FreeBSTR(bstrMethodName);
      return;
    }

    IntPtr bstrTimeout = Marshal.StringToBSTR("Timeout");
    VARIANT varTimeout = new() { vt = 3 /* VT_I4 */, lVal = 0 };

    IntPtr bstrBrightness = Marshal.StringToBSTR("Brightness");
    VARIANT varBrightness = new() { vt = 17 /* VT_UI1 */, bVal = (byte)brightness };

    IntPtr* vtableInParams = *(IntPtr**)pInParams;
    var put = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, VARIANT*, int, int>)vtableInParams[5];

    put(pInParams, bstrTimeout, 0, &varTimeout, 0);
    put(pInParams, bstrBrightness, 0, &varBrightness, 0);

    IntPtr bstrQueryLang = Marshal.StringToBSTR("WQL");
    IntPtr bstrQuery = Marshal.StringToBSTR("SELECT * FROM WmiMonitorBrightnessMethods");
    IntPtr pEnum = IntPtr.Zero;

    var execQuery = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr*, int>)vtableSvc[20];
    hr = execQuery(_pSvc, bstrQueryLang, bstrQuery, 0x20 | 0x10, IntPtr.Zero, &pEnum);

    Marshal.FreeBSTR(bstrQueryLang);
    Marshal.FreeBSTR(bstrQuery);

    if (hr >= 0 && pEnum != IntPtr.Zero)
    {
      IntPtr* vtableEnum = *(IntPtr**)pEnum;
      var next = (delegate* unmanaged[Stdcall]<IntPtr, int, uint, IntPtr*, uint*, int>)vtableEnum[4];
      IntPtr pObj = IntPtr.Zero;
      uint uReturned = 0;
      hr = next(pEnum, -1, 1, &pObj, &uReturned);
      ReleaseComObject(pEnum);

      if (hr >= 0 && uReturned > 0 && pObj != IntPtr.Zero)
      {
        IntPtr bstrPathName = Marshal.StringToBSTR("__PATH");
        VARIANT varPath = new();

        IntPtr* vtableObj = *(IntPtr**)pObj;
        var get = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int, VARIANT*, int*, int*, int>)vtableObj[4];
        hr = get(pObj, bstrPathName, 0, &varPath, null, null);
        ReleaseComObject(pObj);

        if (hr >= 0 && varPath.vt == 8 /* VT_BSTR */ && varPath.bstrVal != IntPtr.Zero)
        {
          var execMethod = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr, IntPtr*, IntPtr*, int>)vtableSvc[24];
          execMethod(_pSvc, varPath.bstrVal, bstrMethodName, 0, IntPtr.Zero, pInParams, null, null);
        }
        VariantClear(ref varPath);
        Marshal.FreeBSTR(bstrPathName);
      }
    }

    ReleaseComObject(pInParams);
    Marshal.FreeBSTR(bstrClassPath);
    Marshal.FreeBSTR(bstrMethodName);
    Marshal.FreeBSTR(bstrTimeout);
    Marshal.FreeBSTR(bstrBrightness);
  }

  private static unsafe void ReleaseComObject(IntPtr pUnk)
  {
    if (pUnk != IntPtr.Zero)
    {
      IntPtr* vtable = *(IntPtr**)pUnk;
      var release = (delegate* unmanaged[Stdcall]<IntPtr, uint>)vtable[2];
      release(pUnk);
    }
  }
}
