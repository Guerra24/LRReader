#if NET
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;

namespace LRReader.UWP.Servicing;

public class CertUtil : IDisposable
{
	private X509Store Store;

	public CertUtil()
	{
		Store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
		bool readWrite = false;
		using (var identity = WindowsIdentity.GetCurrent())
			readWrite = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
		Store.Open(readWrite ? OpenFlags.ReadWrite : OpenFlags.ReadOnly);
	}

	public void Dispose()
	{
		Store.Dispose();
	}

	public async Task<bool> InstallCertificate(Uri uri, string thumb)
	{
		var found = FindCertificate(thumb);
		if (!found)
		{
			using var client = new HttpClient();
			var data = await client.GetByteArrayAsync(uri).ConfigureAwait(false);
			using var cert = X509CertificateLoader.LoadCertificate(data);
			if (!cert.Thumbprint.Equals(thumb.ToUpper()))
				return false;
			Store.Add(cert);
		}
		return true;
	}

	public bool UninstallCertificate(string thumb)
	{
		var col = Store.Certificates.Find(X509FindType.FindByThumbprint, thumb.ToUpper(), false);
		if (col != null && col.Count > 0)
			Store.RemoveRange(col);
		return true;
	}

	public bool FindCertificate(string thumb)
	{
		var col = Store.Certificates.Find(X509FindType.FindByThumbprint, thumb.ToUpper(), false);
		return col != null && col.Count > 0;
	}
}

#endif