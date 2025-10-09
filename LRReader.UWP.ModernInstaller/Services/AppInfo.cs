using System;

namespace LRReader.UWP.Installer.Services;

public record AppInfo(string PackageFamilyName, Uri AppInstallerUrl, CertMeta MainCert, string[] ExpiredCerts, Version Version);

public record CertMeta(Uri Url, string Thumbprint);