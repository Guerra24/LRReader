$xmlFileName = "LRReader.UWP\Package.appxmanifest"      
[xml]$xmlDoc = Get-Content $xmlFileName

$count = & "C:\Program Files\Git\mingw64\bin\git.exe" rev-list --count HEAD
$version = [System.Version]::Parse($xmlDoc.Package.Identity.Version);

$Field = $version.GetType().GetField('_Build','static,nonpublic,instance')
$Field.SetValue($version,$count -as [int])

$xmlDoc.Package.Identity.Version = $version.ToString();

echo "Version:" $xmlDoc.Package.Identity.Version

$xmlDoc.Save("$(Get-Location)/$xmlFileName")