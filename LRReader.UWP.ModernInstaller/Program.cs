using LRReader.UWP.Installer.Services;
using LRReader.UWP.Servicing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.MB;
using static TerraFX.Interop.Windows.SM;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.WS;

namespace LRReader.UWP.Installer;

internal class Program
{
	private static App? _xamlApp = null;

	[STAThread]
	public static unsafe int Main(string[] args)
	{
		if (Environment.OSVersion.Version < new Version(10, 0, 19041, 0))
		{
			var title = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "Not supported".GetPinnableReference()));
			var content = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "LRReader requires Windows 10 20H1 or newer".GetPinnableReference()));

			MessageBoxW(HWND.NULL, content, title, MB_ICONERROR | MB_OK);
			return 0;
		}

		if (!Version.TryParse("{APP_VERSION}", out var version))
			version = new(0, 0, 0, 0);

#if DEBUG
		var appInstallerUrl = "https://s3.guerra24.net/projects/lrr/nightly/LRReader.UWP.appinstaller";
#else
		var appInstallerUrl = "{APP_INSTALLER_URL}";
#endif

		Service.BuildServices(new AppInfo(
			"Guerra24.LRReader_3fr0p4qst6948",
			new Uri(appInstallerUrl),
			new CertMeta(new Uri(CertInfo.CertUrlV2), CertInfo.CertThumbV2),
			[CertInfo.CertThumb],
			version));

		using (var certUtil = Service.Services.GetRequiredService<CertUtil>())
		{
			var appInfo = Service.AppInfo;
			if (args != null && args.Length > 0)
			{
				bool ok = false;
				switch (args[0])
				{
					case "--install-cert":
						ok = certUtil.InstallCertificate(appInfo.MainCert.Url, appInfo.MainCert.Thumbprint).GetAwaiter().GetResult();
						break;
					case "--uninstall-cert":
						ok = certUtil.UninstallCertificate(appInfo.MainCert.Thumbprint);
						foreach (var certThumb in appInfo.ExpiredCerts)
							ok = certUtil.UninstallCertificate(certThumb);
						break;
				}
				return ok ? 0 : -1;
			}

			var lpszClassName = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "LRReaderInstallerClass".GetPinnableReference()));
			var lpWindowName = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "LRReader".GetPinnableReference()));

			WNDCLASSW wc;
			wc.lpfnWndProc = &WndProc;
			wc.hInstance = GetModuleHandleW(null);
			wc.lpszClassName = lpszClassName;
			RegisterClassW(&wc);

			var hwnd = CreateWindowExW(WS_EX_NOREDIRECTIONBITMAP, lpszClassName, lpWindowName, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, 976, 521, HWND.NULL, HMENU.NULL, wc.hInstance, null);

			MSG msg;
			while (GetMessageW(&msg, HWND.NULL, 0, 0))
			{
				bool xamlSourceProcessedMessage = _xamlApp is not null && _xamlApp.PreTranslateMessage(&msg);
				if (!xamlSourceProcessedMessage)
				{
					TranslateMessage(&msg);
					DispatchMessageW(&msg);
				}
			}
		}
		return 0;
	}

	[UnmanagedCallersOnly]
	private unsafe static LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
	{
		switch (uMsg)
		{
			case WM_CREATE:
				_xamlApp = new(hWnd);
				break;
			case WM_SIZE:
				{
					var dpi = GetDpiForWindow(hWnd);
					var border = GetSystemMetricsForDpi(SM_CXFRAME, dpi) + GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);
					var visualBorder = (int)Math.Ceiling(1 * ((float)dpi / USER_DEFAULT_SCREEN_DPI));
					var y = 0;
					var height = (int)HIWORD(lParam);
					if (IsZoomed(hWnd))
					{
						y = border - visualBorder;
						height -= border - visualBorder;
					}
					_xamlApp?.OnResize(0, y, LOWORD(lParam), height);
				}
				break;
			case WM_SETTINGCHANGE:
			case WM_THEMECHANGED:
				_xamlApp?.ProcessCoreWindowMessage(uMsg, wParam, lParam);
				break;
			case WM_GETMINMAXINFO:
				{
					var dpi = (float)GetDpiForWindow(hWnd) / USER_DEFAULT_SCREEN_DPI;
					var mmi = (MINMAXINFO*)lParam;
					mmi->ptMinTrackSize.x = (int)(976 * dpi);
					mmi->ptMinTrackSize.y = (int)(521 * dpi);
				}
				break;
			case WM_NCCALCSIZE:
				if (wParam == 1)
				{
					var dpi = GetDpiForWindow(hWnd);
					var border = GetSystemMetricsForDpi(SM_CXFRAME, dpi) + GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);
					//var visualBorder = GetSystemMetricsForDpi(SM_CYBORDER, dpi);
					NCCALCSIZE_PARAMS* param = (NCCALCSIZE_PARAMS*)lParam;
					param->rgrc[0].left += border;
					//param->rgrc[0].top += IsZoomed(hWnd) ? border - visualBorder : 0;
					param->rgrc[0].top += 0;
					param->rgrc[0].right -= border;
					param->rgrc[0].bottom -= border;
					break;
				}
				else
				{
					return DefWindowProcW(hWnd, uMsg, wParam, lParam);
				}
			case WM_NCHITTEST:
				{
					LRESULT dwmResult;
					if (DwmDefWindowProc(hWnd, uMsg, wParam, lParam, &dwmResult))
						return dwmResult;

					var x = LOWORD(lParam);
					var y = HIWORD(lParam);
					var ret = DefWindowProcW(hWnd, uMsg, wParam, lParam);

					if (ret == HTCLIENT)
					{
						RECT rc;
						GetWindowRect(hWnd, &rc);
						var dpi = GetDpiForWindow(hWnd);
						var border = GetSystemMetricsForDpi(SM_CXFRAME, dpi) + GetSystemMetricsForDpi(SM_CXPADDEDBORDER, dpi);
						if (y < rc.top + border)
							return HTTOP;
						else
							return HTCAPTION;
					}
					return ret;
				}
			case WM_SETFOCUS:
				_xamlApp?.OnSetFocus();
				break;
			case WM_DESTROY:
				_xamlApp?.Dispose();
				_xamlApp = null;
				PostQuitMessage(0);
				break;
			default:
				return DefWindowProcW(hWnd, uMsg, wParam, lParam);
		}
		return 0;
	}

}
