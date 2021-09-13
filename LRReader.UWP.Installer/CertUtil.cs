using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Linq;

namespace LRReader.UWP.Installer
{
	public class CertUtil
	{
		private static X509Store Store;

		public static void Open(OpenFlags flags = OpenFlags.ReadOnly)
		{
			Store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
			Store.Open(flags);
		}

		public static void Close()
		{
			Store.Close();
		}

		public static async Task<bool> InstallCertificate(string url, string thumb)
		{
			var found = FindCertificate(thumb);
			if (!found)
			{
				var file = Path.GetTempFileName();
				using (var client = new WebClient())
					await client.DownloadFileTaskAsync(new Uri(url), file);
				var cert = new X509Certificate2(file);
				if (!cert.Thumbprint.Equals(thumb.ToUpper()))
					return false;
				Store.Add(cert);
				File.Delete(file);
			}
			return true;
		}

		public static bool UninstallCertificate(string thumb)
		{
			var col = Store.Certificates.Find(X509FindType.FindByThumbprint, thumb.ToUpper(), false);
			if (col != null && col.Count > 0)
				Store.RemoveRange(col);
			return true;
		}

		public static bool FindCertificate(string thumb)
		{
			var col = Store.Certificates.Find(X509FindType.FindByThumbprint, thumb.ToUpper(), false);
			return col != null && col.Count > 0;
		}
	}
}
