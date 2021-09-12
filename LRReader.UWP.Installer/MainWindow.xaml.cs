using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

		public MainWindow()
		{
			InitializeComponent();
			if (Variables.AppInstallerUrl.Equals("{APP_INSTALLER_URL}"))
				Variables.AppInstallerUrl = "https://s3.guerra24.net/projects/lrr/nightly/LRReader.UWP.appinstaller";
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var presentationSource = PresentationSource.FromVisual((Visual)sender);
			presentationSource.ContentRendered += Window_ContentRendered;
			if (Environment.OSVersion.Version < new Version(10, 0, 17763, 0))
				Error.Text = "LRReader requires Windows 10 1809";
			else
				InstallButton.Visibility = Visibility.Visible;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			var accent = new AccentPolicy();
			accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLIC;
			accent.GradientColor = 0xD92C2C2C;

			var accentStructSize = Marshal.SizeOf(accent);

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new WindowCompositionAttributeData();
			data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
			data.SizeOfData = accentStructSize;
			data.Data = accentPtr;

			SetWindowCompositionAttribute(((HwndSource)sender).Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}

		private async void Install_Click(object sender, RoutedEventArgs e)
		{
			(sender as Button).Visibility = Visibility.Collapsed;
			TitleText.Visibility = Progress.Visibility = Visibility.Visible;
			Progress.IsIndeterminate = true;

			await Task.Run(async () =>
			{
				using (var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine))
				{
					store.Open(OpenFlags.ReadWrite);
					var col = store.Certificates.Find(X509FindType.FindByThumbprint, Variables.CertThumb, false);
					if (!(col != null && col.Count > 0))
					{
						var cert = Path.GetTempFileName();
						using (var client = new WebClient())
							await client.DownloadFileTaskAsync(new Uri(Variables.CertUrl), cert);
						store.Add(new X509Certificate2(cert));
						File.Delete(cert);
					}
				}
			});

			var done = await Task.Run(async () =>
			{
				var pm = new PackageManager();
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
				return false;
			});
			if (done)
			{
				Close();
			}
			else
			{
				TitleText.Visibility = Visibility.Collapsed;
			}
		}
	}
}
