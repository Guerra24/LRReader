using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace LRReader.UWP.Servicing.CertInstaller;

internal static class Program
{
	static void Main(string[] args)
	{
		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
		{
			ProcessUtil.LaunchAdmin(Assembly.GetExecutingAssembly().Location).GetAwaiter().GetResult();
			return;
		}

		CertUtil.Open(OpenFlags.ReadWrite);
		CertUtil.InstallCertificate(CertInfo.CertUrlV2, CertInfo.CertThumbV2).GetAwaiter().GetResult();
		CertUtil.Close();
	}
}
