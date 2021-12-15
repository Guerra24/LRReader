using ModernWpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Windows.Management.Deployment;

namespace LRReader.UWP.Installer
{

	public partial class MainWindow : Window
	{
		private HwndSource hwnd;

		private PackageManager pm;

		private bool CertFound;

		private bool IsWin11 = Environment.OSVersion.Version >= new Version(10, 0, 22000, 0);

		public MainWindow()
		{
			InitializeComponent();
			// Win32 Magic
			var interop = new WindowInteropHelper(this);
			interop.EnsureHandle();
			hwnd = HwndSource.FromHwnd(interop.Handle);
			hwnd.AddHook(WndProc);
			SetTheme(hwnd.Handle);
			EnableMica(hwnd.Handle);
			User32.SetWindowLongPtr(hwnd.Handle, User32.GWL_STYLE, User32.GetWindowLongPtr(hwnd.Handle, User32.GWL_STYLE) & ~User32.WS_SYSMENU);
			User32.SetWindowPos(hwnd.Handle, IntPtr.Zero, 0, 0, 0, 0, User32.SWP_NOZORDER | User32.SWP_NOMOVE | User32.SWP_NOSIZE | User32.SWP_NOACTIVATE | User32.SWP_DRAWFRAME);

			if (Variables.AppInstallerUrl.Equals("{APP_INSTALLER_URL}"))
				Variables.AppInstallerUrl = "https://s3.guerra24.net/projects/lrr/nightly/LRReader.UWP.appinstaller";
			if (Variables.Version.Equals("{APP_VERSION}"))
				Variables.Version = "0.0.0.0";

			if (IsWin11)
				Icon1.FontFamily = Icon2.FontFamily = Icon3.FontFamily = new FontFamily("Segoe Fluent Icons");

			string title;
			if (Variables.Version.Contains("Nightly"))
				title = $"LRReader {Variables.Version}";
			else
				title = $"LRReader {Variables.Version.Substring(0, Variables.Version.LastIndexOf('.'))}";
			Title = WindowTitle.Text = title;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Environment.OSVersion.Version < new Version(10, 0, 17763, 0))
			{
				Error.Text = "LRReader requires Windows 10 1809";
			}
			else
			{
				pm = await Task.Run(() => new PackageManager());
				CertFound = CertUtil.FindCertificate(Variables.CertThumb);
				var pkg = pm.FindPackagesForUser(string.Empty, Variables.PackageFamilyName).FirstOrDefault();
				if (pkg != null && CertFound)
				{
					var ver = new Version(pkg.Id.Version.Major, pkg.Id.Version.Minor, pkg.Id.Version.Build, pkg.Id.Version.Revision);
					if (Variables.Version.Contains("Nightly") || new Version(Variables.Version) > ver)
					{
						InstallApp.Content = "Upgrade";
						InstallApp.Visibility = Visibility.Visible;
					}
					UninstallApp.Visibility = Visibility.Visible;
				}
				else if (CertFound)
					UninstallCert.Visibility = InstallApp.Visibility = Visibility.Visible;
				else
					InstallApp.Visibility = Visibility;
			}
		}

		private async void Install_Click(object sender, RoutedEventArgs e)
		{
			Buttons.Visibility = Visibility.Collapsed;
			TitleText.Visibility = Progress.Visibility = Visibility.Visible;
			Progress.IsIndeterminate = true;

			if (!CertFound)
			{
				var result = await LaunchAdmin("--install-cert");
				if (result != 0)
				{
					TitleText.Visibility = Progress.Visibility = Visibility.Collapsed;
					Progress.IsIndeterminate = false;
				}
				switch (result)
				{
					case 1:
						Error.Text = "An invalid certificate has been detected";
						return;
					case -99:
						Error.Text = "Admin permissions are required for certificate installation";
						return;
				}
			}
			bool done = true;
			if (pm.FindPackagesForUser(string.Empty, Variables.PackageFamilyName).FirstOrDefault() == null)
			{
				done = await Task.Run(async () =>
				{
					try
					{
						var installTask = pm.AddPackageByAppInstallerFileAsync(new Uri(Variables.AppInstallerUrl), AddPackageByAppInstallerOptions.ForceTargetAppShutdown, pm.GetDefaultPackageVolume());
						Dispatcher.Invoke(() =>
						{
							Progress.IsIndeterminate = false;
							TaskbarProgress.ProgressState = TaskbarItemProgressState.Normal;
						});
						installTask.Progress = (asyncInfo, prog) => Dispatcher.Invoke(() => TaskbarProgress.ProgressValue = Progress.Value = prog.percentage / 100d);
						var result = await installTask.AsTask();
						Dispatcher.Invoke(() =>
						{
							Progress.Visibility = Visibility.Collapsed;
							TaskbarProgress.ProgressState = TaskbarItemProgressState.Normal;
						});
						if (result.IsRegistered)
							return true;
						else
						{
							Dispatcher.Invoke(() =>
							{
								TitleText.Visibility = Visibility.Collapsed;
								Error.Text = result.ErrorText;
							});
						}
						return false;
					}
					catch (Exception i)
					{
						Dispatcher.Invoke(() =>
						{
							if (i.HResult == -2147009281)
							{
								TitleText.Visibility = Visibility.Collapsed;
								Progress.Visibility = Visibility.Collapsed;
								Error.Text = "Enable \"Sideload apps\" and rerun the installer";
								OpenSettings.Visibility = Visibility.Visible;
							}
							else
							{
								TitleText.Visibility = Visibility.Collapsed;
								Progress.Visibility = Visibility.Collapsed;
								Error.Text = string.Format("Unable to install app. Error: 0x{0:X}", i.HResult);
							}
						});
						return false;
					}
				});
			}
			if (done)
			{
				var pkg = pm.FindPackagesForUser(string.Empty, Variables.PackageFamilyName).FirstOrDefault();
				var app = (await pkg.GetAppListEntriesAsync()).FirstOrDefault();
				await app.LaunchAsync();
				Close();
			}
		}

		private async void UninstallCert_Click(object sender, RoutedEventArgs e)
		{
			Buttons.Visibility = Visibility.Collapsed;
			if (await LaunchAdmin("--uninstall-cert") == -99)
				Error.Text = "Admin permissions are required for certificate removal";
			else
				Close();
		}

		private async void Uninstall_Click(object sender, RoutedEventArgs e)
		{
			Buttons.Visibility = Visibility.Collapsed;
			var pkg = pm.FindPackagesForUser(string.Empty, Variables.PackageFamilyName).FirstOrDefault();
			if (pkg != null)
			{
				try
				{
					await pm.RemovePackageAsync(pkg.Id.FullName);
				}
				catch (Exception)
				{
					Error.Text = "Unable to uninstall, please uninstall from start menu";
				}
			}
			if (await LaunchAdmin("--uninstall-cert") == -99)
				Error.Text = "Admin permissions are required for certificate removal";
			else
				Close();
		}

		private async Task<int> LaunchAdmin(string command)
		{
			var process = new Process();
			process.StartInfo.Verb = "runas";
			process.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
			process.StartInfo.Arguments = command;
			try
			{
				await process.StartAndWaitForExitAsync();
				return process.ExitCode;
			}
			catch (Win32Exception) { }
			return -99;
		}

		private void Window_ActualThemeChanged(object sender, RoutedEventArgs e)
		{
			if (hwnd != null)
				SetTheme(hwnd.Handle);
		}

		private void EnableMica(IntPtr hwnd)
		{
			if (!IsWin11)
				return;
			int trueValue = 0x01;
			Dwmapi.DwmSetWindowAttribute(hwnd, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
		}

		private void SetTheme(IntPtr hwnd)
		{
			if (!IsWin11)
			{
				if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
				{
					LeftBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B2B2B"));
					RightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
				}
				else
				{
					LeftBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBFBFB"));
					RightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
				}
			}
			else
			{
				int trueValue = 0x01;
				int falseValue = 0x00;
				if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
					Dwmapi.DwmSetWindowAttribute(hwnd, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
				else
					Dwmapi.DwmSetWindowAttribute(hwnd, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));
			}
		}

		// Fix broken borders by hand
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case User32.WM_NCCALCSIZE:
					handled = true;
					if (wParam.ToInt32() == 1)
					{
						var nccsp = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NCCALCSIZE_PARAMS));
						nccsp.rgrc0.Top += 0;
						nccsp.rgrc0.Bottom -= 1;
						nccsp.rgrc0.Left += 1;
						nccsp.rgrc0.Right -= 1;
						Marshal.StructureToPtr(nccsp, lParam, true);
					}
					return lParam;
				case User32.WM_NCHITTEST:
					handled = true;
					var point = GetPoint(lParam);
					RECT rect;
					User32.GetWindowRect(hwnd, out rect);
					if (point.Y < rect.Top + 34 && point.X < rect.Right - 50)
						return new IntPtr(User32.HTCAPTION);
					return new IntPtr(User32.HTCLIENT);
			}
			return IntPtr.Zero;
		}

		private Point GetPoint(IntPtr _xy)
		{
			uint xy = unchecked(IntPtr.Size == 8 ? (uint)_xy.ToInt64() : (uint)_xy.ToInt32());
			int x = unchecked((short)xy);
			int y = unchecked((short)(xy >> 16));
			return new Point(x, y);
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("ms-settings:developers");
		}
	}
}
