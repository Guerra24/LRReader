using LRReader.UWP.Installer.Interop;
using LRReader.UWP.Installer.Views;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using WinRT;
using static TerraFX.Interop.Windows.SM;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;

namespace LRReader.UWP.Installer;

public partial class App : Application, IDisposable
{
	private HWND _hwnd = default;
	private HWND _coreHwnd = default;

	private WindowsXamlManager _xamlManager = null!;
	private CoreWindow _coreWindow = null!;

	private XamlCompositionSurface _surface = null!;
	private XamlCompositionSurface _titlebar = null!;

	public App(HWND hwnd)
	{
		_hwnd = hwnd;
		InitializeXaml();
	}

	private unsafe void InitializeXaml()
	{
		// Is this needed anymore? maybe for older builds?
		LoadLibraryA((sbyte*)Unsafe.AsPointer(ref Unsafe.AsRef(in "twinapi.appcore.dll\0"u8.GetPinnableReference())));
		LoadLibraryA((sbyte*)Unsafe.AsPointer(ref Unsafe.AsRef(in "threadpoolwinrt.dll\0"u8.GetPinnableReference())));

		_xamlManager = WindowsXamlManager.InitializeForCurrentThread();

		_coreWindow = CoreWindow.GetForCurrentThread();

		using ComPtr<ICoreWindowInterop> interop = default;
		ThrowIfFailed(((IUnknown*)((IWinRTObject)_coreWindow).NativeObject.ThisPtr)->QueryInterface(__uuidof<ICoreWindowInterop>(), (void**)interop.GetAddressOf()));
		ThrowIfFailed(interop.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _coreHwnd)));

		SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
		((IXamlSourceTransparency)(object)Window.Current).SetIsBackgroundTransparent(true);

		_surface = new(_hwnd, () =>
		{
			var frame = new Frame();
			frame.Navigate(typeof(InstallerPage));
			return frame;
		});

		_titlebar = new(_hwnd, () =>
		{
			var panel = new StackPanel() { Orientation = Orientation.Horizontal };
			panel.Children.Add(new TextBlock() { Text = "LRReader 0.0.0", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(13, -2, 0, 0), FontSize = 12 });
			return panel;
		});
		_titlebar.ClickThrough();
	}

	public void Dispose()
	{
		_xamlManager.Dispose();
	}

	internal void OnResize(int x, int y, int width, int height)
	{
		var dpi = GetDpiForWindow(_hwnd);
		var border = GetSystemMetricsForDpi(SM_CXFRAME, dpi) + GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);
		var caption = GetSystemMetricsForDpi(SM_CYCAPTION, dpi);
		var visualBorder = (int)Math.Ceiling(1 * ((float)dpi / USER_DEFAULT_SCREEN_DPI));

		_titlebar.Resize(x, y + visualBorder, width, caption + border + visualBorder);

		_surface.Resize(x, y + border + caption + visualBorder * 2, width, height - border - caption - visualBorder * 2);

		SendMessageA(_coreHwnd, WM_SIZE, (WPARAM)width, height);
	}

	internal void ProcessCoreWindowMessage(uint message, WPARAM wParam, LPARAM lParam)
	{
		SendMessageA(_coreHwnd, message, wParam, lParam);
	}

	internal void OnSetFocus()
	{
		//_surface.OnSetFocus();
	}

	internal unsafe bool PreTranslateMessage(MSG* msg)
	{
		return _surface.PreTranslateMessage(msg) || _titlebar.PreTranslateMessage(msg);
	}

}
