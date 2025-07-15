using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace AudioDeviceSwitcher
{
    [ComImport]
    [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IPolicyConfig
    {
        uint GetMixFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out WaveFormatExtensible ppFormat);
        uint GetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bDefault, out WaveFormatExtensible ppFormat);
        uint ResetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
        uint SetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.LPStruct)] WaveFormatExtensible pEndpointFormat, [In][MarshalAs(UnmanagedType.LPStruct)] WaveFormatExtensible pMixFormat);
        uint GetProcessingPeriod([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bDefault, out long pmftDefaultPeriod, out long pmftMinimumPeriod);
        uint SetProcessingPeriod([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, long pmftPeriod);
        uint GetShareMode([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out DeviceShareMode pMode);
        uint SetShareMode([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] DeviceShareMode mode);
        uint GetPropertyValue([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bFxStore, ref PropertyKey pKey, out PropVariant pv);
        uint SetPropertyValue([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bFxStore, [In] ref PropertyKey pKey, ref PropVariant pv);
        uint SetDefaultEndpoint([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.U4)] ERole role);
        uint SetEndpointVisibility([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bVisible);
    }

    public enum ERole
    {
        eConsole = 0,
        eMultimedia = eConsole + 1,
        eCommunications = eMultimedia + 1,
        ERole_enum_count = eCommunications + 1
    }

    public enum DeviceShareMode
    {
        Shared,
        Exclusive
    }

}
