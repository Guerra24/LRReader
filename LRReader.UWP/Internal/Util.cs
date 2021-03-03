using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Internal
{
	public static class Util
	{

		private static readonly Uri checkUri = new Uri("check:check");

		public static Version GetAppVersion()
		{
			Package package = Package.Current;
			PackageId packageId = package.Id;
			PackageVersion version = packageId.Version;

			return new Version(version.Major, version.Minor, version.Build, version.Revision);
		}

		public static string GetPackageFamilyName()
		{
			return AppInfo.Current.PackageFamilyName;
		}

		public static async Task<bool> OpenInBrowser(Uri uri)
		{
			return await Launcher.LaunchUriAsync(uri);
		}

		public static async Task<bool> CheckAppInstalled(string package)
		{
			try
			{
				var result = await Launcher.QueryUriSupportAsync(checkUri, LaunchQuerySupportType.Uri, package);
				switch (result)
				{
					case LaunchQuerySupportStatus.Available:
					case LaunchQuerySupportStatus.NotSupported:
						return true;
					default:
						return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
