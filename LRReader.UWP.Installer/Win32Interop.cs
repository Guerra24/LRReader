using System;
using System.Runtime.InteropServices;

namespace LRReader.UWP.Installer
{
	[Flags]
	public enum DwmWindowAttribute : uint
	{
		DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
		DWMWA_WINDOW_CORNER_PREFERENCE = 33,
		DWMWA_SYSTEMBACKDROP_TYPE = 38,
		DWMWA_MICA_EFFECT = 1029
	}

	[Flags]
	enum DWM_SYSTEMBACKDROP_TYPE
	{
		DWMSBT_AUTO = 0,
		DWMSBT_DISABLE = 1, // None
		DWMSBT_MAINWINDOW = 2, // Mica
		DWMSBT_TRANSIENTWINDOW = 3, // Acrylic
		DWMSBT_TABBEDWINDOW = 4 // Tabbed
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MARGINS
	{
		public int cxLeftWidth;
		public int cxRightWidth;
		public int cyTopHeight;
		public int cyBottomHeight;
	};

	public static class Dwmapi
	{

		[DllImport("dwmapi.dll")]
		public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

		[DllImport("dwmapi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
	}

	public enum AccentState : uint
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLIC = 4,
		ACCENT_INVALID_STATE = 5
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AccentPolicy
	{
		public AccentState AccentState;
		public uint AccentFlags;
		public uint GradientColor;
		public uint AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	public enum WindowCompositionAttribute : uint
	{
		WCA_ACCENT_POLICY = 19
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPOS
	{
		public IntPtr hwnd;
		public IntPtr hwndinsertafter;
		public int x, y, cx, cy;
		public int flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NCCALCSIZE_PARAMS
	{
		public RECT rgrc0, rgrc1, rgrc2;
		public WINDOWPOS lppos;
	}

	public static class User32
	{
		public const int GWL_STYLE = -16;
		public const int WS_SYSMENU = 0x80000;
		public const int WS_CAPTION = 0x00C00000;
		public const int WS_THICKFRAME = 0x00040000;
		public const int WS_TILED = 0x00000000;

		public const int SWP_NOSIZE = 0x1;
		public const int SWP_NOMOVE = 0x2;
		public const int SWP_NOZORDER = 0x4;
		public const int SWP_NOACTIVATE = 0x10;
		public const int SWP_DRAWFRAME = 0x20;

		public const int WM_NCHITTEST = 0x0084;
		public const int WM_NCCALCSIZE = 0x0083;

		public const int HTCLIENT = 1;
		public const int HTCAPTION = 2;
		public const int HTBORDER = 18;
		public const int HTBOTTOM = 15;
		public const int HTBOTTOMLEFT = 16;
		public const int HTBOTTOMRIGHT = 17;
		public const int HTLEFT = 10;
		public const int HTRIGHT = 11;
		public const int HTTOP = 12;
		public const int HTTOPLEFT = 13;
		public const int HTTOPRIGHT = 14;

		[DllImport("user32.dll")]
		public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
		[DllImport("user32.dll")]
		public static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		public static extern long SetWindowLongPtr(IntPtr hWnd, int nIndex, long dwNewLong);
		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int cx, int cy, uint flags);
		[DllImport("user32.dll")]
		public static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
	}

	public static class UxTheme
	{

		[DllImport("uxtheme.dll", EntryPoint = "#135")]
		public static extern int SetPreferredAppMode(int mode);

	}
}
