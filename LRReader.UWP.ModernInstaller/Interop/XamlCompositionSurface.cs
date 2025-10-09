using System;
using System.Runtime.CompilerServices;
using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using WinRT;
using static TerraFX.Interop.Windows.GWL;
using static TerraFX.Interop.Windows.SWP;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WS;

namespace LRReader.UWP.Installer.Interop;

public class XamlCompositionSurface
{
	private HWND _xamlHwnd = default;

	private DesktopWindowXamlSource _desktopWindowXamlSource = null!;

	private ComPtr<IDesktopWindowXamlSourceNative2> _nativeSource = default;

	public XamlCompositionSurface(HWND parent, Func<UIElement> content)
	{
		InitializeSurface(parent, content);
	}

	private unsafe void InitializeSurface(HWND parent, Func<UIElement> content)
	{
		_desktopWindowXamlSource = new();

		ThrowIfFailed(((IUnknown*)((IWinRTObject)_desktopWindowXamlSource).NativeObject.ThisPtr)->QueryInterface(__uuidof<IDesktopWindowXamlSourceNative2>(), (void**)_nativeSource.GetAddressOf()));

		ThrowIfFailed(_nativeSource.Get()->AttachToWindow(parent));
		ThrowIfFailed(_nativeSource.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _xamlHwnd)));

		_desktopWindowXamlSource.Content = content();
	}

	public void Resize(int x, int y, int width, int height)
	{
		SetWindowPos(_xamlHwnd, HWND.NULL, x, y, width, height, SWP_SHOWWINDOW | SWP_NOACTIVATE | SWP_NOZORDER);
	}

	public void OnSeSettFocus()
	{
		SetFocus(_xamlHwnd);
	}

	public void ClickThrough()
	{
		nint dwExStyle = GetWindowLongPtrA(_xamlHwnd, GWL_EXSTYLE);
		dwExStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
		SetWindowLongPtrA(_xamlHwnd, GWL_EXSTYLE, dwExStyle);
	}

	public unsafe bool PreTranslateMessage(MSG* msg)
	{
		BOOL result = false;

		_nativeSource.Get()->PreTranslateMessage(msg, &result);

		return result;
	}

}
