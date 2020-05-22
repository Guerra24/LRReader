using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace LRReader.Internal
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

		public static async Task<bool> OpenInBrowser(Uri uri)
		{
			return await Launcher.LaunchUriAsync(uri);
		}
	}
}
