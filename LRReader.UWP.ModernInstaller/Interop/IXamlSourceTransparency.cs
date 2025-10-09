using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace LRReader.UWP.Installer.Interop;

[GeneratedComInterface]
[Guid("06636C29-5A17-458D-8EA2-2422D997A922")]
public partial interface IXamlSourceTransparency
{
	void GetIids(out int iidCount, out IntPtr iids);
	void GetRuntimeClassName(out IntPtr className);
	void GetTrustLevel(out int trustLevel);
	[return: MarshalAs(UnmanagedType.I1)] bool GetIsBackgroundTransparent();
	void SetIsBackgroundTransparent([MarshalAs(UnmanagedType.I1)] bool isBackgroundTransparent);
}
