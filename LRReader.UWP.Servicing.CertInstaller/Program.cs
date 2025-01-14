using System.Security.Cryptography.X509Certificates;

namespace LRReader.UWP.Servicing.CertInstaller;

internal static class Program
{
	static void Main(string[] args)
	{
		CertUtil.Open(OpenFlags.ReadWrite);
		CertUtil.InstallCertificate(CertInfo.CertUrlV2, CertInfo.CertThumbV2).GetAwaiter().GetResult();
		CertUtil.Close();
	}
}
