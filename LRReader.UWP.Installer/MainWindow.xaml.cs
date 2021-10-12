using Microsoft.Win32;
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
			if (IsWin11)
				try
				{
					RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
					if (registryKey.GetValue("EnableTransparency").ToString().Equals("0"))
						IsWin11 = false;
				}
				catch (Exception) { IsWin11 = false; }

			InitializeComponent();
			var interop = new WindowInteropHelper(this);
			interop.EnsureHandle();
			hwnd = HwndSource.FromHwnd(interop.Handle);
			SetTheme(hwnd.Handle);
			EnableMica(hwnd.Handle);

			if (Variables.AppInstallerUrl.Equals("{APP_INSTALLER_URL}"))
				Variables.AppInstallerUrl = "https://s3.guerra24.net/projects/lrr/nightly/LRReader.UWP.appinstaller";
			if (Variables.Version.Equals("{APP_VERSION}"))
				Variables.Version = "0.0.0.0";

			if (Environment.OSVersion.Version >= new Version(10, 0, 22000, 0))
			{
				Height = 517;
				LeftBorder.Margin = RightBorder.Margin = new Thickness(0, 0, 0, 3);
				WindowChrome.SetWindowChrome(this, new WindowChrome { CaptionHeight = 32, GlassFrameThickness = new Thickness(0, 1, 0, 0), NonClientFrameEdges = NonClientFrameEdges.Bottom, UseAeroCaptionButtons = false });
				Icon1.FontFamily = Icon2.FontFamily = Icon3.FontFamily = new FontFamily("Segoe Fluent Icons");
			}

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
				if (pm.FindPackagesForUser(string.Empty, Variables.PackageFamilyName).FirstOrDefault() != null && CertFound)
					UninstallApp.Visibility = Visibility.Visible;
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
					catch (Exception)
					{
						// Fallback to appinstaller in case of failure for whatever reason...
						Process.Start($"ms-appinstaller:?source={Variables.AppInstallerUrl}");
						Dispatcher.Invoke(() => Close());
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
			var accent = new AccentPolicy();
			accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLIC;
			accent.GradientColor = 0x00000000;

			var accentStructSize = Marshal.SizeOf(accent);

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new WindowCompositionAttributeData();
			data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;

			User32.SetWindowCompositionAttribute(hwnd, ref data);

			Marshal.FreeHGlobal(accentPtr);

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

	}
}
