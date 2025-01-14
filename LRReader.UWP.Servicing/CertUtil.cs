using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace LRReader.UWP.Servicing;

public static class CertUtil
{
	private static X509Store Store = null!;

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
			using var client = new HttpClient();
			var data = await client.GetByteArrayAsync(new Uri(url));
			var cert = new X509Certificate2(data);
			if (!cert.Thumbprint.Equals(thumb.ToUpper()))
				return false;
			Store.Add(cert);
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
