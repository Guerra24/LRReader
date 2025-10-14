using LRReader.UWP.Installer.Interop;
using LRReader.UWP.Installer.Views;
using LRReader.UWP.Installer.Views.Controls;
using MrmPatcher;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using WinRT;
using static TerraFX.Interop.Windows.SM;
using static TerraFX.Interop.Windows.SW;
using static TerraFX.Interop.Windows.SWP;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.WinRT.WinRT;

namespace LRReader.UWP.Installer;

public partial class App : Application, IDisposable
{
	private HWND _hwnd = default;
	private HWND _coreHwnd = default;

	private WindowsXamlManager _xamlManager = null!;
	private CoreWindow _coreWindow = null!;

	internal List<XamlCompositionSurface> surfaces = new();

	private XamlCompositionSurface _surface = null!;
	private XamlCompositionSurface _titlebar = null!;

	public App(HWND hwnd)
	{
		_hwnd = hwnd;
		InitializeXaml();
	}

	private unsafe void InitializeXaml()
	{
		RoInitialize(RO_INIT_TYPE.RO_INIT_SINGLETHREADED);
		// Is this needed anymore? maybe for older builds?
		LoadLibraryW((char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "twinapi.appcore.dll".GetPinnableReference())));
		LoadLibraryW((char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "threadpoolwinrt.dll".GetPinnableReference())));

		// If this is NAOT we need to patch Mrm
		using (new MrmPatcherHelper())
		{
			_xamlManager = WindowsXamlManager.InitializeForCurrentThread();
		}

		_coreWindow = CoreWindow.GetForCurrentThread();

		using ComPtr<ICoreWindowInterop> interop = default;
		((IUnknown*)((IWinRTObject)_coreWindow).NativeObject.ThisPtr)->QueryInterface(__uuidof<ICoreWindowInterop>(), (void**)interop.GetAddressOf());
		interop.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _coreHwnd));

		SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
		((IXamlSourceTransparency)(object)Window.Current).SetIsBackgroundTransparent(true);

		_surface = new(_hwnd, () => new InstallerPage());

		_titlebar = new(_hwnd, () => new Titlebar());
		_titlebar.ClickThrough();

		OnChangeTheme(RequestedTheme == ApplicationTheme.Dark ? true : false);

		var margins = new MARGINS();
		margins.cxLeftWidth = -1;
		margins.cxRightWidth = -1;
		margins.cyBottomHeight = -1;
		margins.cyTopHeight = -1;
		DwmExtendFrameIntoClientArea(_hwnd, &margins);

		if (Environment.OSVersion.Version >= new Version(10, 0, 22621, 0))
		{
			var type = DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW;
			DwmSetWindowAttribute(_hwnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, &type, sizeof(DWM_SYSTEMBACKDROP_TYPE));
		}

		SetWindowPos(_hwnd, HWND.NULL, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
		ShowWindow(_hwnd, SW_SHOWNORMAL);
	}

	public void Dispose()
	{
		_xamlManager.Dispose();
	}

	public void OnResize(int x, int y, int width, int height)
	{
		var dpi = GetDpiForWindow(_hwnd);
		var border = GetSystemMetricsForDpi(SM_CXFRAME, dpi) + GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);
		var caption = GetSystemMetricsForDpi(SM_CYCAPTION, dpi);
		var visualBorder = (int)Math.Ceiling(1 * ((float)dpi / USER_DEFAULT_SCREEN_DPI));

		_titlebar.Resize(x, y + visualBorder, width, caption + border + visualBorder);

		_surface.Resize(x, y + border + caption + visualBorder * 2, width, height - border - caption - visualBorder * 2);

		SendMessageW(_coreHwnd, WM_SIZE, (WPARAM)width, height);
	}

	public void ProcessCoreWindowMessage(uint message, WPARAM wParam, LPARAM lParam)
	{
		SendMessageW(_coreHwnd, message, wParam, lParam);
	}

	public void OnSetFocus()
	{
		//_surface.OnSetFocus();
	}

	public unsafe void OnChangeTheme(bool darkmode)
	{
		BOOL val = darkmode;
		DwmSetWindowAttribute(_hwnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, &val, (uint)sizeof(BOOL));
	}

	public unsafe bool PreTranslateMessage(MSG* msg)
	{
		foreach(var surface in surfaces)
			if (surface.PreTranslateMessage(msg))
				return true;
		return false;
	}

}
