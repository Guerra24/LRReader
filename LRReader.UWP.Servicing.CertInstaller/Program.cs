using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;

namespace LRReader.UWP.Servicing.CertInstaller;

internal static class Program
{
	static async Task Main(string[] args)
	{
#if DEBUG
		if (!System.Diagnostics.Debugger.IsAttached)
			System.Diagnostics.Debugger.Launch();
#endif
		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
		{
			// Workaround second process deadlocking when downloading cert
			// Remove once 1809 is dropped
			using var client = new HttpClient();
			var data = await client.GetByteArrayAsync(new Uri(CertInfo.CertUrlV2));
			var file = Path.GetTempFileName();
			File.WriteAllBytes(file, data);
			var res = await ProcessUtil.LaunchAdmin(Assembly.GetExecutingAssembly().Location, file);
			if (res == -99)
				File.Delete(file);
			return;
		}
		if (File.Exists(args[0]))
		{
			var data = File.ReadAllBytes(args[0]);
			CertUtil.Open(OpenFlags.ReadWrite);
			CertUtil.InstallCertificate(data, CertInfo.CertThumbV2);
			CertUtil.Close();
			File.Delete(args[0]);
		}
	}
}
