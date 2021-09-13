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

	internal enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLIC = 4,
		ACCENT_INVALID_STATE = 5
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AccentPolicy
	{
		public AccentState AccentState;
		public uint AccentFlags;
		public uint GradientColor;
		public uint AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	internal enum WindowCompositionAttribute
	{
		WCA_ACCENT_POLICY = 19
	}

	public partial class MainWindow : Window
	{
		[DllImport("user32.dll")]
		internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

		private HwndSource hwnd;

		private PackageManager pm;

		private bool CertFound;

		public MainWindow()
		{
			InitializeComponent();
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
			hwnd = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			SetAcrylic(hwnd.Handle);

			if (Environment.OSVersion.Version < new Version(10, 0, 17763, 0))
			{
				Error.Text = "LRReader requires Windows 10 1809";
			}
			else
			{
				pm = await Task.Run(() => new PackageManager());
				CertFound = CertUtil.FindCertificate(Variables.CertThumb);
				if (pm.FindPackagesForUser(string.Empty, "Guerra24.LRReader_3fr0p4qst6948").FirstOrDefault() != null && CertFound)
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
			var done = await Task.Run(async () =>
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
				{
					var pkg = pm.FindPackagesForUser(string.Empty, "Guerra24.LRReader_3fr0p4qst6948").FirstOrDefault();
					var app = (await pkg.GetAppListEntriesAsync()).FirstOrDefault();
					await app.LaunchAsync();
					return true;
				}
				else
				{
					Dispatcher.Invoke(() =>
					{
						TitleText.Visibility = Visibility.Collapsed;
						Error.Text = result.ErrorText;
					});
				}
				return false;
			});
			if (done)
				Close();
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
			var pkg = pm.FindPackagesForUser(string.Empty, "Guerra24.LRReader_3fr0p4qst6948").FirstOrDefault();
			if (pkg != null)
			{
				await pm.RemovePackageAsync(pkg.Id.FullName);
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
				SetAcrylic(hwnd.Handle);
		}

		private void SetAcrylic(IntPtr hwnd)
		{
			if (Environment.OSVersion.Version < new Version(10, 0, 22000, 0))
			{
				if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
				{
					LeftBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
					RightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C2C"));
				}
				else
				{
					LeftBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
					RightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9"));
				}
				return;
			}
			var accent = new AccentPolicy();
			accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLIC;
			if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
				accent.GradientColor = 0xD92C2C2C;
			else
				accent.GradientColor = 0xA9FCFCFC;

			var accentStructSize = Marshal.SizeOf(accent);

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new WindowCompositionAttributeData();
			data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;

			SetWindowCompositionAttribute(hwnd, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}

	}
}
