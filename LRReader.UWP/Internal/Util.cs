using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Internal
{
	public static class Util
	{
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

		public static async Task<BitmapImage> ByteToBitmap(byte[] bytes, BitmapImage image = null)
		{
			if (bytes == null)
				return null;
			if (bytes.Length == 0)
				return null;
			using (var stream = new InMemoryRandomAccessStream())
			{
				await stream.WriteAsync(bytes.AsBuffer());
				stream.Seek(0);
				if (image == null)
					image = new BitmapImage();
				await image.SetSourceAsync(stream);
				return image;
			}
		}
	}
}
